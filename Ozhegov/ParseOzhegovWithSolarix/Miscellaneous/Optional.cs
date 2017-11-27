namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal class Optional<T> : IOptional<T>
    {
        private Optional()
        {
        }

        public Optional(T value)
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

        public static readonly Optional<T> None = new Optional<T>();

        private readonly T _value;
    }
}
