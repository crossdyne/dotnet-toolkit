namespace Crossdyne.Toolkit.Primitives
{
 public readonly record struct Maybe<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        private Maybe(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public bool HasValue => _hasValue;

        public bool IsNone => !_hasValue;

        public T Value => HasValue
            ? _value
            : throw new InvalidOperationException("Невозможно получить доступ к значению «None» у Maybe.");

 
        public static Maybe<T> None => new();

        /// <summary>
        /// Creates Maybe with the value (Some). If the passed value is null, Maybe.None will be returned.
        /// </summary>
        /// <param name="value">The value for encapsulation.</param>
        public static Maybe<T> Some(T? value) => value is null ? None : new Maybe<T>(value);

        /// <summary>
        /// Returns the encapsulated value if it exists, otherwise it returns the default value.
        /// </summary>
        /// <param name="defaultValue">Значение, которое будет возвращено, если Maybe является None.</param>
        public T GetValueOrDefault(T defaultValue = default!) => HasValue? _value : defaultValue;

        /// <summary>
        /// Performs one of two actions, depending on whether the value exists.
        /// </summary>
        /// <param name="onSome">An action performed with a value, if it exists.</param>
        /// <param name="onNone">Action performed if the value is missing.</param>
        public void Match(Action<T> onSome, Action onNone)
        {
            if (HasValue)
                onSome(_value);
            else
                onNone();
        }

        // <summary>
        /// Projects the value into a new form, if it exists.
        /// </summary>
        /// <param name="onSome">A function executed with a value, if it exists.</param>
        /// <param name="onNone">A function that returns a result if the value is missing.</param>
        public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) => HasValue ? onSome(_value) : onNone();

        /// <summary>
        /// Converts the value inside Maybe to a new type if the value exists.
        /// If Maybe is None, it returns Maybe.None for the new type.
        /// </summary>
        /// <typeparam name="TResult">The type of the conversion result.</typeparam>
        /// <param name="map">Conversion function.</param>
        public Maybe<TResult> Map<TResult>(Func<T, TResult> map) => HasValue ? Maybe<TResult>.Some(map(_value)) : Maybe<TResult>.None;

        /// <summary>
        /// Binds Maybe to the result of a function that also returns Maybe.
        /// It is used for chains of calls returning Maybe.
        /// </summary>
        /// <typeparam name="TResult">The type of value in the resulting Maybe.</typeparam>
        /// <param name="bind">Binding function.</param>
        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> bind) => HasValue ? bind(_value) : Maybe<TResult>.None;

        public override string ToString() => HasValue ? $"Some({_value})" : "None";

        public static implicit operator Maybe<T>(T? value) => Some(value);
    }
}