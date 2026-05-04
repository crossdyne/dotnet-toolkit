
using Crossdyne.Toolkit.Validation;

namespace Crossdyne.Toolkit.Tests
{
    public class GuardClauseExtensionsTests
    {
        #region That (bool condition, Func<Exception> exceptionFactory)

        [Fact]
        public void That_WithTrueConditionAndExceptionFactory_ThrowsProvidedException()
        {
            var expectedException = new InvalidOperationException("Test error");

            var actualException = Assert.Throws<InvalidOperationException>(() => Guard.Against.That(condition: true, exceptionFactory: () => expectedException));

            Assert.Same(expectedException, actualException);
        }

        [Fact]
        public void That_WithFalseConditionAndExceptionFactory_DoesNotThrow()
        {
            var factoryWasCalled = false;
            Func<Exception> factory = () => 
            { 
                factoryWasCalled = true; 
                return new InvalidOperationException(); 
            };

            var exception = Record.Exception(() => Guard.Against.That(condition: false, exceptionFactory: factory));

            Assert.Null(exception);
            Assert.False(factoryWasCalled, "Factory should not be called when condition is false");
        }

        #endregion

        #region That (bool condition, string message)

        [Fact]
        public void That_WithTrueConditionAndMessage_ThrowsArgumentExceptionWithMessage()
        {
            const string expectedMessage = "Custom validation failed";

            var ex = Assert.Throws<ArgumentException>(() => Guard.Against.That(condition: true, message: expectedMessage));

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void That_WithFalseConditionAndMessage_DoesNotThrow()
        {
            var exception = Record.Exception(() => Guard.Against.That(condition: false, message: "Any message"));

            Assert.Null(exception);
        }

        #endregion

        #region Null<T>

        [Fact]
        public void Null_WithNullReference_ThrowsArgumentNullException()
        {
            string? input = null;
            const string paramName = "testParam";

            var ex = Assert.Throws<ArgumentNullException>(() => Guard.Against.Null(input, paramName));

            Assert.Equal(paramName, ex.ParamName);
        }

        [Fact]
        public void Null_WithNonNullReference_DoesNotThrow()
        {
            var input = "valid string";

            var exception = Record.Exception(() => Guard.Against.Null(input, parameterName: "testParam"));

            Assert.Null(exception);
        }

        [Fact]
        public void Null_WithNullParameterName_ThrowsArgumentNullExceptionWithNullParamName()
        {
            string? input = null;

            var ex = Assert.Throws<ArgumentNullException>(() => Guard.Against.Null(input, parameterName: null!));

            Assert.Null(ex.ParamName); // ArgumentNullException allows null ParamName
        }

        [Fact]
        public void Null_WithValueType_DoesNotThrow_BecauseValueTypesCannotBeNull()
        {
            var input = 42;

            var exception = Record.Exception(() => Guard.Against.Null(input, parameterName: "testParam"));

            Assert.Null(exception);
            // Note: This is expected behavior. The method is designed for reference types.
        }

        #endregion

        #region NullOrEmpty

        [Fact]
        public void NullOrEmpty_WithNullString_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty(input: null!, parameterName: "testParam"));

            Assert.Equal("testParam", ex.ParamName);
            Assert.Contains("cannot be null or empty", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void NullOrEmpty_WithEmptyString_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty(input: "", parameterName: "testParam"));

            Assert.Equal("testParam", ex.ParamName);
            Assert.Contains("cannot be null or empty", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void NullOrEmpty_WithWhitespaceString_DoesNotThrow()
        {
            var exception = Record.Exception(() => Guard.Against.NullOrEmpty(input: "   ", parameterName: "testParam"));

            Assert.Null(exception);
        }

        [Fact]
        public void NullOrEmpty_WithValidString_DoesNotThrow()
        {
            var exception = Record.Exception(() => Guard.Against.NullOrEmpty(input: "valid", parameterName: "testParam"));

            Assert.Null(exception);
        }

        [Fact]
        public void NullOrEmpty_WithNullParameterName_ThrowsWithNullParamName()
        {
            var ex = Assert.Throws<ArgumentException>(() => Guard.Against.NullOrEmpty(input: null!, parameterName: null!));

            Assert.Null(ex.ParamName);
        }

        #endregion

        #region OutOfRange

        [Theory]
        [InlineData(-1, 0, 10, true)]   // below range
        [InlineData(0, 0, 10, false)]   // lower bound (inclusive)
        [InlineData(5, 0, 10, false)]   // inside range
        [InlineData(10, 0, 10, false)]  // upper bound (inclusive)
        [InlineData(11, 0, 10, true)]   // above range
        [InlineData(int.MinValue, -100, 100, true)]
        [InlineData(int.MaxValue, -100, 100, true)]
        public void OutOfRange_Throws_WhenValueOutsideInclusiveBounds(
            int value, int from, int to, bool shouldThrow)
        {
            if (shouldThrow)
            {
                var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Against.OutOfRange(value, from, to, "testParam"));
                Assert.Equal("testParam", ex.ParamName);
                Assert.Equal(value, ex.ActualValue);
                Assert.Contains(from.ToString(), ex.Message);
                Assert.Contains(to.ToString(), ex.Message);
            }
            else
            {
                var exception = Record.Exception(() => Guard.Against.OutOfRange(value, from, to, "testParam"));
                Assert.Null(exception);
            }
        }

        [Fact]
        public void OutOfRange_WithNullParameterName_ThrowsWithNullParamName()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Against.OutOfRange(input: 999, rangeFrom: 0,rangeTo: 10, parameterName: null!));

            Assert.Null(ex.ParamName);
        }

        [Fact]
        public void OutOfRange_WithReversedRange_ThrowsForAnyValue() => Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Against.OutOfRange(input: 5, rangeFrom: 10, rangeTo: 0, parameterName: "testParam"));

        #endregion

        #region Extension method usage verification

        [Fact]
        public void AllMethods_WorkAsExtensionMethods_OnGuardClauseInstance()
        {
            // This test verifies that the methods can be called as extensions
            // and that the first parameter (GuardClause) is properly ignored.

            var ex1 = Record.Exception(() => Guard.Against.That(condition: false, message: "msg"));
            Assert.Null(ex1);

            var ex2 = Record.Exception(() => Guard.Against.Null(input: "not null", parameterName: "param"));
            Assert.Null(ex2);

            var ex3 = Record.Exception(() => Guard.Against.NullOrEmpty(input: "ok", parameterName: "param"));
            Assert.Null(ex3);

            var ex4 = Record.Exception(() => Guard.Against.OutOfRange(input: 5, rangeFrom: 0, rangeTo: 10, "param"));
            Assert.Null(ex4);
        }

        #endregion
    }
}