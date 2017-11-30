namespace ParseOzhegovWithSolarix.Miscellaneous
{
    public interface IOptional<out T>
    {
        bool HasValue { get; }

        T Value { get; }
    }
}
