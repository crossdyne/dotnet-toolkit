using Quantropic.Toolkit.Primitives;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Quantropic.Toolkit.Results
{
    public abstract class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IImmutableList<Error> Errors { get; }

        protected Result(bool isSuccess, IEnumerable<Error> errors)
        {
            Errors = errors.ToImmutableList();

            if (Errors.Contains(null))
                throw new InvalidOperationException("You can't create empty errors..");

            if (isSuccess && Errors.Any())
                throw new InvalidOperationException("You cannot create a successful result with errors.");

            if (!isSuccess && !Errors.Any())
                throw new InvalidOperationException("You can't create a failed result without errors.");

            IsSuccess = isSuccess;
        }

        /*-- Фабричные методы ----------------------------------------------------*/

        public static Result<Unit> Success() => Result<Unit>.Success(Unit.Value);
        public static Result<Unit> Failure(Error error) => Result<Unit>.Failure(error);
        public static Result<Unit> Failure(IEnumerable<Error> errors) => Result<Unit>.Failure(errors);

        /// <summary>
        /// Комбинирует несколько результатов. Возвращает Failure, если есть хотя бы одна ошибка.
        /// </summary>
        public static Result<Unit> Combine(params Result[] results)
        {
            var failed = results.Where(r => r.IsFailure).ToList();

            return failed.Count == 0
                ? Success()
                : Failure(failed.SelectMany(r => r.Errors));
        }

        /// <summary>
        /// Комбинирует результаты со значениями. Возвращает массив значений при успехе.
        /// </summary>
        public static Result<TValue[]> Combine<TValue>(params Result<TValue>[] results)
        {
            var failed = results.Where(r => r.IsFailure).ToList();

            return failed.Count == 0
                ? Result<TValue[]>.Success(results.Select(r => r.Value).ToArray())
                : Result<TValue[]>.Failure(failed.SelectMany(r => r.Errors));
        }

        /// <summary>
        /// Преобразует успешный Unit-результат в результат со значением.
        /// </summary>
        public Result<TValue> Map<TValue>(Func<TValue> projector) => IsSuccess ? Result<TValue>.Success(projector()) : Result<TValue>.Failure(Errors);

        /// <summary>
        /// Цепочечное преобразование для Unit-результата.
        /// </summary>
        public Result<TValue> Bind<TValue>(Func<Result<TValue>> binder) => IsSuccess ? binder() : Result<TValue>.Failure(Errors);

        /// <summary>
        /// Выполняет одну из двух функций в зависимости от состояния результата.
        /// </summary>
        public TResult Match<TResult>(Func<TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure) => IsSuccess ? onSuccess() : onFailure(Errors);

        /// <summary>
        /// Выполняет одно из двух действий в зависимости от состояния результата.
        /// </summary>
        public void Switch(Action onSuccess, Action<IReadOnlyList<Error>> onFailure)
        {
            if (IsSuccess)
                onSuccess();
            else
                onFailure(Errors);
        }

        /*-- Форматирование ошибок ----------------------------------------------*/

        public string StringMessage => BuildMessage(error => $"Код: {error.Code.Code}:{error.Code.Name}. Причина: {error.Message}");

        private string BuildMessage(Func<Error, string> messageSelector)
        {
            if (Errors.Count == 0) 
                return "No errors.";

            var sb = new StringBuilder();
            sb.AppendLine("Errors:");

            for (int i = 0; i < Errors.Count; i++)
            {
                sb.Append($"{i + 1}) ").AppendLine(messageSelector(Errors[i]));
            }

            return sb.ToString();
        }

        public override string ToString() => BuildMessage(error =>
            $"Status: [{IsSuccess}] Code: [{error.Code} - {error.Code}] Info: [{error.Message}]");
    }

    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        private Result(TValue? value, bool isSuccess, IEnumerable<Error> errors) : base(isSuccess, errors) => _value = value;

        /// <summary>
        ///  Возвращает значение. Бросает InvalidOperationException, если результат неуспешен.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Нельзя получить значение из неуспешного результата.");

        /// <summary>
        /// Попытка получить значение. Возвращает false, если результат неуспешен.
        /// </summary>
        public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
        {
            value = _value;
            return IsSuccess;
        }

        /*-- Фабричные методы ----------------------------------------------------*/

        public static Result<TValue> Success([AllowNull] TValue value) => new(value, true, []);

        public new static Result<TValue> Failure(Error error) => new(default, false, [error]);

        public new static Result<TValue> Failure(IEnumerable<Error> errors) => new(default, false, errors);

        /// <summary>
        /// Преобразует успешный результат через проектор.
        /// </summary>
        public Result<TResult> Map<TResult>(Func<TValue, TResult> projector) => IsSuccess ? Result<TResult>.Success(projector(Value)) : Result<TResult>.Failure(Errors);

        /// <summary>
        /// Преобразует успешный результат через асинхронный проектор.
        /// </summary>
        public async Task<Result<TResult>> MapAsync<TResult>(Func<TValue, Task<TResult>> projector) => IsSuccess ? Result<TResult>.Success(await projector(Value)) : Result<TResult>.Failure(Errors);

        /// <summary>
        /// Цепочечное преобразование: если успех — выполняет binder, иначе пропускает ошибку.
        /// </summary>
        public Result<TResult> Bind<TResult>(Func<TValue, Result<TResult>> binder) => IsSuccess ? binder(Value) : Result<TResult>.Failure(Errors);

        /// <summary>
        /// Асинхронная версия Bind.
        /// </summary>
        public async Task<Result<TResult>> BindAsync<TResult>(Func<TValue, Task<Result<TResult>>> binder) => IsSuccess ? await binder(Value) : Result<TResult>.Failure(Errors);

        /// <summary>
        /// Выполняет одну из двух функций в зависимости от состояния результата.
        /// </summary>
        public TResult Match<TResult>(
            Func<TValue, TResult> onSuccess,
            Func<IReadOnlyList<Error>, TResult> onFailure) =>
            IsSuccess ? onSuccess(Value) : onFailure(Errors);

        /// <summary>
        /// Выполняет одно из двух действий в зависимости от состояния результата.
        /// </summary>
        public void Switch(Action<TValue> onSuccess, Action<IReadOnlyList<Error>> onFailure)
        {
            if (IsSuccess)
                onSuccess(Value);
            else
                onFailure(Errors);
        }

        /*-- Implicit operators (с осторожностью) -------------------------------*/

        public static implicit operator Result<TValue>([AllowNull] TValue value) => Success(value);
        public static implicit operator Result<TValue>(Error error) => Failure(error);

        /*-- Переопределённые методы --------------------------------------------*/

        public override string ToString() => IsSuccess
            ? $"Success({Value?.GetType().Name})"
            : $"Failure({string.Join(", ", Errors.Select(e => e.Message))})";
    }
}