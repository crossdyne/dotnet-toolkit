using Quantropic.Toolkit.Validation;

namespace Quantropic.Toolkit.Tests
{
    public class RuleValidatorTests
    {
        [Fact]
        public void Validate_ReturnsEmptyList_WhenNoRulesAdded()
        {
            var validator = new RuleValidator();

            var errors = validator.Validate();

            Assert.Empty(errors);
        }

        [Fact]
        public void AddRule_AddsError_WhenConditionReturnsTrue()
        {
            var validator = new RuleValidator();
            validator.AddRule(() => true, "Error Message");

            var errors = validator.Validate();

            Assert.Single(errors);
            Assert.Contains("Error Message", errors);
        }

        [Fact]
        public void AddRule_NoError_WhenConditionReturnsFalse()
        {
            var validator = new RuleValidator();
            validator.AddRule(() => false, "Error Message");

            var errors = validator.Validate();

            Assert.Empty(errors);
        }

        [Fact]
        public void Required_AddsError_WhenObjectIsNull()
        {
            var validator = new RuleValidator();
            object? nullObject = null;

            validator.Required(nullObject, "Object is required");
            var errors = validator.Validate();

            Assert.Single(errors);
            Assert.Contains("Object is required", errors);
        }

        [Fact]
        public void Required_NoError_WhenObjectIsNotNull()
        {
            var validator = new RuleValidator();
            object validObject = new object();

            validator.Required(validObject, "Object is required");
            var errors = validator.Validate();

            Assert.Empty(errors);
        }

        [Fact]
        public void MinCount_AddsError_WhenCollectionCountIsLessThanMin()
        {
            var validator = new RuleValidator();
            var collection = new List<int> { 1, 2 }; // Count = 2

            validator.MinCount(collection, 5, "Too few items");
            var errors = validator.Validate();

            Assert.Single(errors);
            Assert.Contains("Too few items", errors);
        }

        [Fact]
        public void MinCount_NoError_WhenCollectionCountIsEqualToMin()
        {
            var validator = new RuleValidator();
            var collection = new List<int> { 1, 2, 3 }; // Count = 3

            validator.MinCount(collection, 3, "Too few items");
            var errors = validator.Validate();

            Assert.Empty(errors);
        }

        [Fact]
        public void MinCount_NoError_WhenCollectionCountIsGreaterThanMin()
        {
            var validator = new RuleValidator();
            var collection = new List<int> { 1, 2, 3, 4 }; // Count = 4

            validator.MinCount(collection, 3, "Too few items");
            var errors = validator.Validate();

            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_MaintainsOrder_OfErrorMessages()
        {
            var validator = new RuleValidator();
            
            validator
                .AddRule(() => true, "First Error")
                .AddRule(() => false, "Second Error (Should not appear)")
                .AddRule(() => true, "Third Error");

            var errors = validator.Validate();

            Assert.Equal(2, errors.Count);
            Assert.Equal("First Error", errors[0]);
            Assert.Equal("Third Error", errors[1]);
        }

        [Fact]
        public void FluentInterface_MethodsReturnSameInstance()
        {
            var validator = new RuleValidator();

            var result1 = validator.AddRule(() => false, "");
            var result2 = validator.Required(new object(), "");
            var result3 = validator.MinCount(new List<int>(), 0, "");

            Assert.Same(validator, result1);
            Assert.Same(validator, result2);
            Assert.Same(validator, result3);
        }

        [Fact]
        public void MinCount_Throws_WhenCollectionIsNull()
        {
            var validator = new RuleValidator();
            ICollection<int>? nullCollection = null;

            validator.MinCount(nullCollection!, 1, "Error");
            
            var exception = Record.Exception(() => validator.Validate());
            
            Assert.NotNull(exception);
            Assert.IsType<System.NullReferenceException>(exception);
        }
    }
}