namespace Quantropic.Toolkit.Validation
{
 public sealed class RuleValidator
    {
        private readonly List<(Func<bool> IsInvalid, string ErrorMessage)> _rules = [];

        public RuleValidator AddRule(Func<bool> condition, string errorMessage)
        {
            _rules.Add((condition, errorMessage));
            return this;
        }

        public RuleValidator Required(object? obj, string errorMessage)
        {
            _rules.Add((() => obj is null, errorMessage));
            return this;
        }

        public RuleValidator MinCount<T>(ICollection<T> collection, int minCount, string errorMessage)
        {
            _rules.Add((() => collection.Count < minCount, errorMessage));
            return this;
        }

        public List<string> Validate() => [.. _rules.Where(rule => rule.IsInvalid()).Select(rule => rule.ErrorMessage)];
    }
}