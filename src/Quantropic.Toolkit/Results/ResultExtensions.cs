using Quantropic.Toolkit.Primitives;

namespace Quantropic.Toolkit.Results
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Выполняет действие при успехе и возвращает тот же результат (для side-effects).
        /// </summary>
        public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> action)
        {
            if (result.IsSuccess)
                action(result.Value);
            return result;
        }

        /// <summary>
        /// Выполняет действие при ошибке (логирование, например).
        /// </summary>
        public static Result<TValue> TapFailure<TValue>(this Result<TValue> result, Action<IReadOnlyList<Error>> action)
        {
            if (result.IsFailure)
                action(result.Errors);
            return result;
        }

        /// <summary>
        /// Преобразует ошибку в другой тип ошибки.
        /// </summary>
        public static Result<TValue> MapError<TValue>(this Result<TValue> result, Func<Error, Error> mapper) =>
            result.IsSuccess
                ? result
                : Result<TValue>.Failure(result.Errors.Select(mapper));

        /// <summary>
        /// Возвращает значение по умолчанию, если результат неуспешен.
        /// </summary>
        public static TValue GetValueOrDefault<TValue>(this Result<TValue> result, TValue? defaultValue = default) =>
            result.IsSuccess ? result.Value : defaultValue!;

        /// <summary>
        /// Возвращает значение или бросает исключение при неудаче.
        /// </summary>
        public static TValue GetValueOrThrow<TValue>(this Result<TValue> result, Func<IReadOnlyList<Error>, Exception>? exceptionFactory = null) =>
            result.IsSuccess
                ? result.Value
                : throw (exceptionFactory?.Invoke(result.Errors) ?? new InvalidOperationException(result.StringMessage));

        /// <summary>
        /// Асинхронная обработка: если успех — выполняет onSuccess, иначе onFailure.
        /// </summary>
        public static async Task<TResult> MatchAsync<TValue, TResult>(
            this Task<Result<TValue>> resultTask,
            Func<TValue, Task<TResult>> onSuccess,
            Func<IReadOnlyList<Error>, Task<TResult>> onFailure)
        {
            var result = await resultTask;
            return result.IsSuccess
                ? await onSuccess(result.Value)
                : await onFailure(result.Errors);
        }

        /// <summary>
        /// Конвертирует Task<Result> в Result<Unit> для удобства.
        /// </summary>
        public static async Task<Result<Unit>> ToUnitResult(this Task<Result> task) =>
            (Result<Unit>)await task;
    }
}
