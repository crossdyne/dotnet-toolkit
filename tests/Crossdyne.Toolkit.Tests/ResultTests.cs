using Crossdyne.Toolkit.Primitives;
using Crossdyne.Toolkit.Results;
using Xunit.Abstractions;

namespace Crossdyne.Toolkit.Tests
{
    public class ResultTests(ITestOutputHelper output)
    {
        ITestOutputHelper _output = output;

        #region TestData

        internal sealed record User(string Id, string Name);

        private User GetUser(string id = "1", string name = "Кирил") => new User(id, name);

        private IEnumerable<User> GetUsers()
            => new List<User>()
            {
                new User(Guid.NewGuid().ToString(), "Иван"),
                new User(Guid.NewGuid().ToString(), "Вика"),
                new User(Guid.NewGuid().ToString(), "Максим"),
            };

        #endregion

        #region Success

        [Fact]
        public void Success_WhenAllSuccessfulResults_ReturnSuccess()
        {
            var result = Result.Success();

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
        }

        [Fact] 
        public void Success_WhenSuccessCreateUnitValue_ReturnSuccess()
        {
            var result = Result.Success();

            Assert.NotNull(result);
            Assert.Equal(Unit.Value, result.Value);
        }

        [Fact]
        public void Success_WhenTryGetValueWithError_ThrowInvalidOperationException()
        {
            var result = Result.Failure(new Error(ErrorCode.Null, "Объект имеет нуль ссылку"));

            Assert.NotNull(result);
            Assert.True(result.IsFailure);
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void Success_WhenTryGetValueWithErrors_ThrowInvalidOperationException()
        {
            List<Error> errors =
            [
                new Error(ErrorCode.Null, "Объект имеет нуль ссылку"),
                new Error(ErrorCode.NotFound, "Объект не найден")
            ];
            var result = Result.Failure(errors);

            Assert.NotNull(result);
            Assert.True(result.IsFailure);
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        #endregion

        #region Failure

        [Fact]
        public void Failure_WhenHaveError_ReturnFailure()
        {
            var result = Result.Failure(new Error(ErrorCode.Null, "Объект имеет нуль ссылку"));

            Assert.NotNull(result);
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            _output.WriteLine(result.StringMessage);
            Assert.Contains($"1) Код: {ErrorCode.Null.Code}:{ErrorCode.Null.Name}. Причина: Объект имеет нуль ссылку", result.StringMessage);
        }

        [Fact]
        public void Failure_WhenHaveErrors_ReturnFailure()
        {
            List<Error> errors =
            [
                new Error(ErrorCode.Null, "Объект имеет нуль ссылку"),
                new Error(ErrorCode.NotFound, "Объект не найден")
            ];

            var result = Result.Failure(errors);

            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Failure_WhenTryCreateFailureWithOutError_ThrowInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => Result.Failure(error: null!));

        [Fact]
        public void Failure_WhenTryCreateFailureWithOutErrors_ThrowInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => Result.Failure(errors: Array.Empty<Error>()));

        #endregion

        #region ImmutableList

        [Fact]
        public void ImmutableList_WhenCorrectAddErrors_CheckCountElementsMoreThanZero()
        {
            var firstError = new Error(ErrorCode.Null, "Объект имеет нуль ссылку.");
            var secondError = new Error(ErrorCode.NotFound, "Объект не найден.");
            var thirdError = new Error(ErrorCode.Empty, "Там все пусто.");

            List<Error> errors = [ firstError, secondError, thirdError ];

            var result = Result.Failure(errors);

            Assert.True(result.Errors.Count > 0);
            Assert.Equal(3, result.Errors.Count);
            Assert.Equal(result.Errors[0], firstError);
        }

        #endregion

        #region Combine

        [Fact]
        public void Combine_WhenAllSuccess_ReturnSuccessWithUsersValues()
        {
            var successFirstResult = Result.Success();
            var successSecondResult = Result.Success();

            var combineResult = Result.Combine(successFirstResult, successSecondResult);

            Assert.Equal(2, combineResult.Value.Length);
            Assert.True(combineResult.IsSuccess);
        }

        [Fact]
        public void Combine_WhenOneFailure_ReturnFailure()
        {
            var successFirstResult = Result.Success();
            var failureFirstResult = Result.Failure(new Error(ErrorCode.Null, "Ссылки нету"));

            var combineResult = Result.Combine(successFirstResult, failureFirstResult);

            Assert.Throws<InvalidOperationException>(() => combineResult.Value);
            Assert.True(combineResult.IsFailure);
            Assert.Contains("Ссылки нету", combineResult.StringMessage);
        }

        [Fact]
        public void Combine_WhenParamIsEmpty_ReturnSuccess()
        {
            var combineResult = Result.Combine([]);

            Assert.True(combineResult.IsSuccess);
        }

        [Fact]
        public void Combine_WhenAnyFailure_ReturnFailureWithAllErrors()
        {
            var failureFirstResult = Result.Failure(new Error(ErrorCode.Null, "Ссылки нету"));
            var failureSecondResult = Result.Failure(new Error(ErrorCode.Delete, "Удалено"));
            var successFirstResult = Result.Success();

            var combineResult = Result.Combine(failureFirstResult, failureSecondResult, successFirstResult);
            var stringMessage = combineResult.StringMessage;

            var firstError = combineResult.Errors[0];
            var secondError = combineResult.Errors[1];

            Assert.Throws<InvalidOperationException>(() => combineResult.Value);
            Assert.True(combineResult.IsFailure);
            Assert.Equal(2, combineResult.Errors.Count);
            Assert.Contains("Ссылки нету", stringMessage);
            Assert.Contains("Удалено", stringMessage);
            Assert.Contains("Ссылки нету", firstError.Message);
            Assert.Contains("Удалено", secondError.Message);
        }

        #endregion

        #region Result (Unit) – дополнительные тесты

        [Fact]
        public void Constructor_WithNullErrorInList_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>  Result.Failure(new Error[] { null! }));
        }

        [Fact]
        public void Combine_WithAllSuccessResults_ReturnsSuccess()
        {
            var r1 = Result.Success();
            var r2 = Result.Success();
            var combined = Result.Combine(r1, r2);

            Assert.True(combined.IsSuccess);
            Assert.Empty(combined.Errors);
        }

        [Fact]
        public void Combine_WithAnyFailure_ReturnsFailureWithAllErrors()
        {
            var error1 = new Error(ErrorCode.Null, "err1");
            var error2 = new Error(ErrorCode.NotFound, "err2");
            var r1 = Result.Success();
            var r2 = Result.Failure(error1);
            var r3 = Result.Failure(error2);

            var combined = Result.Combine(r1, r2, r3);

            Assert.True(combined.IsFailure);
            Assert.Equal(2, combined.Errors.Count);
            Assert.Contains(error1, combined.Errors);
            Assert.Contains(error2, combined.Errors);
        }

        [Fact]
        public void Map_OnSuccess_ReturnsMappedResult()
        {
            var result = Result.Success();
            var mapped = result.Map(() => 42);

            Assert.True(mapped.IsSuccess);
            Assert.Equal(42, mapped.Value);
        }

        [Fact]
        public void Match_OnSuccess_CallsOnSuccess()
        {
            var result = Result.Success();
            var output = result.Match(() => "success", _ => "failure");

            Assert.Equal("success", output);
        }

        [Fact]
        public void Match_OnFailure_CallsOnFailure()
        {
            var error = new Error(ErrorCode.Null, "err");
            var result = Result.Failure(error);
            var output = result.Match(() => "success", errors => $"failure:{errors[0].Message}");

            Assert.Equal("failure:err", output);
        }

        [Fact]
        public void Switch_OnSuccess_ExecutesOnSuccess()
        {
            var result = Result.Success();
            bool executed = false;
            result.Switch(() => executed = true, _ => throw new Exception("Should not be called"));

            Assert.True(executed);
        }

        [Fact]
        public void Switch_OnFailure_ExecutesOnFailure()
        {
            var error = new Error(ErrorCode.Null, "err");
            var result = Result.Failure(error);
            IReadOnlyList<Error>? captured = null;
            result.Switch(() => throw new Exception("Should not be called"), errors => captured = errors);

            Assert.Same(result.Errors, captured);
        }

        [Fact]
        public void StringMessage_FormatsErrorsCorrectly()
        {
            var errors = new[]
            {
                new Error(ErrorCode.Null, "first"),
                new Error(ErrorCode.NotFound, "second")
            };
            var result = Result.Failure(errors);
            var message = result.StringMessage;

            Assert.Contains($"1) Код: {ErrorCode.Null.Code}:{ErrorCode.Null.Name}. Причина: first", message);
            Assert.Contains($"2) Код: {ErrorCode.NotFound.Code}:{ErrorCode.NotFound.Name}. Причина: second", message);
        }

        [Fact]
        public void ToString_FormatsSuccess()
        {
            var result = Result.Success();
            
            Assert.NotEqual("Success()", result.ToString()); 
            Assert.Equal("Success(Unit)", result.ToString());
        }

        [Fact]
        public void ToString_FormatsFailure()
        {
            var error = new Error(ErrorCode.Null, "test");
            var result = Result.Failure(error);
            Assert.Equal("Failure(test)", result.ToString());
        }

        #endregion

        #region Result<TValue> – фабрики и свойства

        [Fact]
        public void Success_WithValue_ReturnsSuccessWithValue()
        {
            var user = GetUser();
            var result = Result<User>.Success(user);

            Assert.True(result.IsSuccess);
            Assert.Equal(user, result.Value);
        }

        [Fact]
        public void Success_WithNullValue_ReturnsSuccessWithNull()
        {
            var result = Result<string?>.Success(null);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public void Failure_WithSingleError_ReturnsFailure()
        {
            var error = new Error(ErrorCode.Null, "msg");
            var result = Result<int>.Failure(error);

            Assert.True(result.IsFailure);
            Assert.Single(result.Errors, error);
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void Failure_WithMultipleErrors_ReturnsFailure()
        {
            var errors = new[] { new Error(ErrorCode.Null, "e1"), new Error(ErrorCode.NotFound, "e2") };
            var result = Result<int>.Failure(errors);

            Assert.True(result.IsFailure);
            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public void TryGetValue_OnSuccess_ReturnsTrueAndValue()
        {
            var result = Result<int>.Success(42);
            var success = result.TryGetValue(out var value);

            Assert.True(success);
            Assert.Equal(42, value);
        }

        [Fact]
        public void TryGetValue_OnFailure_ReturnsFalseAndDefault()
        {
            var result = Result<int>.Failure(new Error(ErrorCode.Null, "err"));
            var success = result.TryGetValue(out var value);

            Assert.False(success);
            Assert.Equal(default, value);
        }

        [Fact]
        public void ImplicitConversion_FromValue_CreatesSuccess()
        {
            Result<User> result = GetUser();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void ImplicitConversion_FromError_CreatesFailure()
        {
            Error error = new(ErrorCode.Null, "err");
            Result<User> result = error;

            Assert.True(result.IsFailure);
            Assert.Single(result.Errors, error);
        }

        #endregion

        #region Result<TValue> – функциональные методы

        [Fact]
        public void Map_OnSuccess_AppliesProjector()
        {
            var result = Result<int>.Success(5);
            var mapped = result.Map(x => x * 2);

            Assert.True(mapped.IsSuccess);
            Assert.Equal(10, mapped.Value);
        }

        [Fact]
        public void Map_OnFailure_ReturnsFailureWithSameErrors()
        {
            var error = new Error(ErrorCode.Null, "err");
            var result = Result<int>.Failure(error);
            var mapped = result.Map(x => x * 2);

            Assert.True(mapped.IsFailure);
            Assert.Single(mapped.Errors, error);
        }

        [Fact]
        public async Task MapAsync_OnSuccess_AppliesAsyncProjector()
        {
            var result = Result<int>.Success(5);
            var mapped = await result.MapAsync(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            });

            Assert.True(mapped.IsSuccess);
            Assert.Equal(10, mapped.Value);
        }

        [Fact]
        public async Task MapAsync_OnFailure_ReturnsFailureWithoutCallingProjector()
        {
            var error = new Error(ErrorCode.Null, "err");
            var result = Result<int>.Failure(error);
            var mapped = await result.MapAsync<int>(async x =>
            {
                await Task.Delay(1);
                throw new Exception("Should not be called");
            });

            Assert.True(mapped.IsFailure);
            Assert.Single(mapped.Errors, error);
        }

        [Fact]
        public void Bind_OnSuccess_ReturnsBinderResult()
        {
            var result = Result<int>.Success(5);
            var bound = result.Bind(x => Result<string>.Success(x.ToString()));

            Assert.True(bound.IsSuccess);
            Assert.Equal("5", bound.Value);
        }

        [Fact]
        public void Bind_OnFailure_ReturnsFailureWithoutCallingBinder()
        {
            var error = new Error(ErrorCode.Null, "err");
            var result = Result<int>.Failure(error);
            var bound = result.Bind<string>(_ => throw new Exception("Binder called"));

            Assert.True(bound.IsFailure);
            Assert.Single(bound.Errors, error);
        }

        [Fact]
        public async Task BindAsync_OnSuccess_ReturnsBinderResult()
        {
            var result = Result<int>.Success(5);
            var bound = await result.BindAsync(async x =>
            {
                await Task.Delay(1);
                return Result<string>.Success(x.ToString());
            });

            Assert.True(bound.IsSuccess);
            Assert.Equal("5", bound.Value);
        }

        [Fact]
        public async Task BindAsync_OnFailure_ReturnsFailureWithoutCallingBinder()
        {
            var error = new Error(ErrorCode.Null, "err");
            var result = Result<int>.Failure(error);
            var bound = await result.BindAsync<string>(_ => throw new Exception("Binder called"));

            Assert.True(bound.IsFailure);
            Assert.Single(bound.Errors, error);
        }

        [Fact]
        public void Match_OnSuccess_ReturnsOnSuccessResult()
        {
            var result = Result<int>.Success(42);
            var output = result.Match(
                onSuccess: v => $"Value: {v}",
                onFailure: _ => "Error");

            Assert.Equal("Value: 42", output);
        }

        [Fact]
        public void Match_OnFailure_ReturnsOnFailureResult()
        {
            var error = new Error(ErrorCode.Null, "msg");
            var result = Result<int>.Failure(error);
            var output = result.Match(
                onSuccess: v => $"Value: {v}",
                onFailure: e => $"Error: {e[0].Message}");

            Assert.Equal("Error: msg", output);
        }

        #endregion
    
        #region ErrorCode 

        [Fact]
        public void ErrorCode_Custom_Success()
        {
            ErrorCode MyError = ErrorCode.Custom(nameof(MyError), 10_001);

            Assert.Equal(10_001, MyError.Code);
            Assert.Equal(nameof(MyError), MyError.Name);
        }

        [Fact]
        public void ErrorCode_Custom_ThrowArgumentOutOfRangeException() => Assert.Throws<ArgumentOutOfRangeException>(() => ErrorCode.Custom("MyError", 156));

        [Fact]
        public void ErrorCode_Custom_ThrowArgumentException() => Assert.Throws<ArgumentException>(() => ErrorCode.Custom("", 156));

        #endregion
    }
}