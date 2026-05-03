namespace Crossdyne.Toolkit.Validation
{
    public static class GuardClauseExtensions
    {
        /// <summary>
        /// Universal Guard for user conditions. It is triggered if the condition is true.
        /// Use it only for rare, specific checks for which there is no standard Guard method.
        /// </summary>
        /// <param name="guardClause">An instance of Guard.</param>
        /// <param name="condition">A condition that, if true, will cause an exception.</param>
        /// <param name="exceptionFactory">Factory for creating an exception if the condition is true.</param>
        /// <exception cref="Exception">An exception created by the factory.</exception>
        public static void That(this GuardClause _, bool condition, Func<Exception> exceptionFactory)
        {
            if (condition)
                throw exceptionFactory();
        }

        /// <summary>
        /// A universal Guard for user conditions with a simple message.
        /// </summary>
        /// <param name="condition">A condition that, if true, will cause an exception.</param>
        /// <param name="message">An exception message.</param>
        /// <exception cref="ArgumentException">An exception with the specified message.</exception>
        public static void That(this GuardClause _, bool condition, string message)
        {
            if (condition)
                throw new ArgumentException(message);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public static void Null<T>(this GuardClause _, T input, string parameterName = null!)
        {
            if (input is null)
                throw new ArgumentNullException(parameterName);
        }

        /// <exception cref="ArgumentException"></exception>
        public static void NullOrEmpty(this GuardClause _, string input, string parameterName = null!)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("The string cannot be null or empty..", parameterName);
        }

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void OutOfRange(this GuardClause _, int input, int rangeFrom, int rangeTo, string parameterName = null!)
        {
            if (input < rangeFrom || input > rangeTo)
                throw new ArgumentOutOfRangeException(parameterName, input, $"The value must be in the range from {rangeFrom} to {rangeTo}.");
        }
    }
}