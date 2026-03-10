using Quantropic.Toolkit.Primitives;

namespace Quantropic.Toolkit.Tests
{
    public class MaybeTests
    {
        #region Construction & Factory Methods
    
        [Fact]
        public void None_Should_HaveValue_False()
        {
            var maybe = Maybe<int>.None;

            Assert.False(maybe.HasValue);
            Assert.True(maybe.IsNone);
        }
    
        [Fact]
        public void Some_WithNonNullValue_Should_HaveValue_True()
        {
            var maybe = Maybe<string>.Some("test");

            Assert.True(maybe.HasValue);
            Assert.False(maybe.IsNone);
        }
    
        [Fact]
        public void Some_WithNullValue_Should_Return_None()
        {
            var maybe = Maybe<string>.Some(null);
    
            Assert.True(maybe.IsNone);
            Assert.False(maybe.HasValue);
        }
    
        [Fact]
        public void ImplicitOperator_WithNonNullValue_Should_Create_Some()
        {
            Maybe<int> maybe = 42;
    
            Assert.True(maybe.HasValue);
            Assert.Equal(42, maybe.Value);
        }
    
        [Fact]
        public void ImplicitOperator_WithNullValue_Should_Create_None()
        {
            Maybe<string> maybe = (string?)null;
    
            Assert.True(maybe.IsNone);
        }
    
        #endregion
    
        #region Value Property
    
        [Fact]
        public void Value_WhenHasValue_Should_Return_Value()
        {
            var expected = "hello";
            var maybe = Maybe<string>.Some(expected);
    
            var actual = maybe.Value;
    
            Assert.Equal(expected, actual);
        }
    
        [Fact]
        public void Value_WhenIsNone_Should_Throw_InvalidOperationException()
        {
            var maybe = Maybe<int>.None;
    
            var exception = Assert.Throws<InvalidOperationException>(() => maybe.Value);
            Assert.Contains("None", exception.Message);
        }
    
        #endregion
    
        #region GetValueOrDefault
    
        [Fact]
        public void GetValueOrDefault_WhenHasValue_Should_Return_Value()
        {
            var maybe = Maybe<int>.Some(100);
    
            var result = maybe.GetValueOrDefault(0);
    
            Assert.Equal(100, result);
        }
    
        [Fact]
        public void GetValueOrDefault_WhenIsNone_Should_Return_DefaultValue()
        {
            var maybe = Maybe<int>.None;
    
            var result = maybe.GetValueOrDefault(42);
    
            Assert.Equal(42, result);
        }
    
        [Fact]
        public void GetValueOrDefault_WhenIsNone_NoArgument_Should_Return_TypeDefault()
        {
            var maybe = Maybe<string>.None;
    
            var result = maybe.GetValueOrDefault();
    
            Assert.Null(result);
        }
    
        [Fact]
        public void GetValueOrDefault_WithValueType_WhenIsNone_Should_Return_TypeDefault()
        {
            var maybe = Maybe<bool>.None;
    
            var result = maybe.GetValueOrDefault();
    
            Assert.False(result); // default(bool) == false
        }
    
        #endregion
    
        #region Match (void)
    
        [Fact]
        public void Match_WhenHasValue_Should_Invoke_OnSome()
        {
            var maybe = Maybe<string>.Some("test");
            var onSomeCalled = false;
            var onNoneCalled = false;
    
            maybe.Match(
                onSome: value => { onSomeCalled = true; Assert.Equal("test", value); },
                onNone: () => onNoneCalled = true
            );
    
            Assert.True(onSomeCalled);
            Assert.False(onNoneCalled);
        }
    
        [Fact]
        public void Match_WhenIsNone_Should_Invoke_OnNone()
        {
            var maybe = Maybe<string>.None;
            var onSomeCalled = false;
            var onNoneCalled = false;
    
            maybe.Match(
                onSome: _ => onSomeCalled = true,
                onNone: () => onNoneCalled = true
            );
    
            Assert.False(onSomeCalled);
            Assert.True(onNoneCalled);
        }
    
        #endregion
    
        #region Match (TResult)
    
        [Fact]
        public void Match_WithResult_WhenHasValue_Should_Invoke_OnSome_And_Return_Result()
        {
            var maybe = Maybe<int>.Some(5);
    
            var result = maybe.Match(
                onSome: x => x * 2,
                onNone: () => -1
            );
    
            Assert.Equal(10, result);
        }
    
        [Fact]
        public void Match_WithResult_WhenIsNone_Should_Invoke_OnNone_And_Return_Result()
        {
            var maybe = Maybe<int>.None;
    
            var result = maybe.Match(
                onSome: x => x * 2,
                onNone: () => -1
            );
    
            Assert.Equal(-1, result);
        }
    
        [Fact]
        public void Match_WithResult_Should_Support_ReferenceType_Transformation()
        {
            var maybe = Maybe<int>.Some(42);
    
            var result = maybe.Match(
                onSome: x => $"Value: {x}",
                onNone: () => "No value"
            );
    
            Assert.Equal("Value: 42", result);
        }
    
        #endregion
    
        #region Map
    
        [Fact]
        public void Map_WhenHasValue_Should_Apply_Function_And_Wrap_Result()
        {
            var maybe = Maybe<int>.Some(10);
    
            var result = maybe.Map(x => x.ToString());
    
            Assert.True(result.HasValue);
            Assert.Equal("10", result.Value);
        }
    
        [Fact]
        public void Map_WhenIsNone_Should_Return_None_Of_NewType()
        {
            var maybe = Maybe<int>.None;
    
            var result = maybe.Map(x => x.ToString());
    
            Assert.True(result.IsNone);
            Assert.Equal(Maybe<string>.None, result);
        }
    
        [Fact]
        public void Map_Should_Support_Chaining()
        {
            var maybe = Maybe<int>.Some(2);
    
            var result = maybe
                .Map(x => x * 3) 
                .Map(x => x + 4);
    
            Assert.Equal(10, result.Value);
        }
    
        [Fact]
        public void Map_Chain_WithNone_Should_Propagate_None()
        {
            var maybe = Maybe<int>.None;
    
            var result = maybe
                .Map(x => x * 3)
                .Map(x => x + 4);
    
            Assert.True(result.IsNone);
        }
    
        #endregion
    
        #region Bind
    
        [Fact]
        public void Bind_WhenHasValue_Should_Apply_Function_And_Return_Result()
        {
            var maybe = Maybe<int>.Some(5);
    
            var result = maybe.Bind(x => Maybe<string>.Some($"num:{x}"));
    
            Assert.True(result.HasValue);
            Assert.Equal("num:5", result.Value);
        }
    
        [Fact]
        public void Bind_WhenIsNone_Should_Return_None_Without_Invoking_Function()
        {
            var maybe = Maybe<int>.None;
            var functionCalled = false;
    
            var result = maybe.Bind(_ =>
            {
                functionCalled = true;
                return Maybe<string>.Some("test");
            });
    
            Assert.False(functionCalled);
            Assert.True(result.IsNone);
        }
    
        [Fact]
        public void Bind_Should_Support_Chaining_With_Maybe_Returning_Functions()
        {
            Maybe<int> Divide(int a, int b) => b == 0 ? Maybe<int>.None : Maybe<int>.Some(a / b);
    
            var result = Maybe<int>.Some(100)
                .Bind(x => Divide(x, 5))   // Some(20)
                .Bind(x => Divide(x, 2));  // Some(10)
    
            Assert.Equal(10, result.Value);
        }
    
        [Fact]
        public void Bind_Chain_WithIntermediateNone_Should_ShortCircuit()
        {
            Maybe<int> Divide(int a, int b) => b == 0 ? Maybe<int>.None : Maybe<int>.Some(a / b);
    
            var result = Maybe<int>.Some(100)
                .Bind(x => Divide(x, 0))   // None (division by zero)
                .Bind(x => Divide(x, 2));  // Should not execute
    
            Assert.True(result.IsNone);
        }
    
        #endregion
    
        #region ToString
    
        [Fact]
        public void ToString_WhenHasValue_Should_Return_Some_Format()
        {
            var maybe = Maybe<string>.Some("hello");
    
            var result = maybe.ToString();
    
            Assert.Equal("Some(hello)", result);
        }
    
        [Fact]
        public void ToString_WhenIsNone_Should_Return_None()
        {
            var maybe = Maybe<int>.None;
    
            var result = maybe.ToString();
    
            Assert.Equal("None", result);
        }
    
        [Fact]
        public void ToString_WithNullValueInside_Should_Not_Be_Possible_But_Handle_Gracefully()
        {
            // Note: Some(null) returns None, so we test with reference type that can be null
            // This test documents the behavior for edge cases
            var maybe = Maybe<string?>.Some("test");
            Assert.Equal("Some(test)", maybe.ToString());
        }
    
        #endregion
    
        #region Equality & Struct Behavior
    
        [Fact]
        public void Two_None_Instances_Should_Be_Equal()
        {
            var maybe1 = Maybe<int>.None;
            var maybe2 = Maybe<int>.None;
    
            Assert.Equal(maybe1, maybe2);
            Assert.True(maybe1 == maybe2);
        }
    
        [Fact]
        public void Two_Some_Instances_With_Same_Value_Should_Be_Equal()
        {
            var maybe1 = Maybe<string>.Some("test");
            var maybe2 = Maybe<string>.Some("test");
    
            Assert.Equal(maybe1, maybe2);
        }
    
        [Fact]
        public void Some_And_None_Should_Not_Be_Equal()
        {
            var maybe1 = Maybe<int>.Some(42);
            var maybe2 = Maybe<int>.None;
    
            Assert.NotEqual(maybe1, maybe2);
        }
    
        [Fact]
        public void RecordStruct_Should_Have_ValueBased_Equality()
        {
            var maybe1 = Maybe<int>.Some(100);
            var maybe2 = Maybe<int>.Some(100);
            var maybe3 = Maybe<int>.Some(200);
    
            Assert.Equal(maybe1.GetHashCode(), maybe2.GetHashCode());
            Assert.NotEqual(maybe1.GetHashCode(), maybe3.GetHashCode());
        }
    
        #endregion
    
        #region Edge Cases & Null Handling
    
        [Fact]
        public void Some_With_ReferenceType_Null_Should_Return_None()
        {
            var maybe = Maybe<object>.Some(null);
    
            Assert.True(maybe.IsNone);
        }
    
        [Fact]
        public void GetValueOrDefault_With_ReferenceType_Default_Should_Work()
        {
            var maybe = Maybe<string>.None;
    
            var result = maybe.GetValueOrDefault("fallback");
    
            Assert.Equal("fallback", result);
        }
    
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(42)]
        public void Some_With_ValueType_Should_Always_HaveValue(int value)
        {
            var maybe = Maybe<int>.Some(value);
    
            Assert.True(maybe.HasValue);
            Assert.Equal(value, maybe.Value);
        }
    
        #endregion
    
        #region Functional Composition Examples
    
        [Fact]
        public void RealWorld_Scenario_ParseAndTransform()
        {
            // Scenario: Parse string to int, then multiply by 2, return as string
            Maybe<int> ParseToInt(string? input) => 
                int.TryParse(input, out var result) 
                    ? Maybe<int>.Some(result) 
                    : Maybe<int>.None;
    
            var input = "21";
            
            var result = Maybe<string>.Some(input)
                .Map(s => ParseToInt(s))
                .Bind(x => x) // flatten Maybe<Maybe<int>> to Maybe<int>
                .Map(x => x * 2)
                .Map(x => x.ToString());
    
            Assert.Equal("42", result.Value);
        }
    
        [Fact]
        public void RealWorld_Scenario_None_Propagation()
        {
            Maybe<int> ParseToInt(string? input) => 
                int.TryParse(input, out var result) 
                    ? Maybe<int>.Some(result) 
                    : Maybe<int>.None;
    
            var input = "not-a-number";
            
            var result = Maybe<string>.Some(input)
                .Map(s => ParseToInt(s))
                .Bind(x => x)
                .Map(x => x * 2);
    
            Assert.True(result.IsNone);
        }
    
        #endregion
    }
}