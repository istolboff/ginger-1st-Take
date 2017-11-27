namespace ParseOzhegovWithSolarix.Miscellaneous
{
    interface IOptional<out T>
    {
        bool HasValue { get; }

        T Value { get; }
    }
}
