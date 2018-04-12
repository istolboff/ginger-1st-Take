namespace ParseOzhegovWithSolarix.Miscellaneous
{
    public static class Optional
    {
        public static Optional<T> None<T>() => new Optional<T>();

        public static Optional<T> Some<T>(T value) => new Optional<T>(value);
    }

    public sealed class Optional<T> : IOptional<T>
    {
        internal Optional()
        {
        }

        internal Optional(T value)
        {
            _value = value;
            HasValue = true;
        }

        public bool HasValue { get; }

        public T Value
        {
            get
            {
                Verify.That(HasValue, () => $"Option<{typeof(T).Name}> does not have value.");
                return _value;
            }
        }

        private readonly T _value;
    }
}
