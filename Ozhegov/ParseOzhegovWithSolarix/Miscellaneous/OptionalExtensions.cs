using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class OptionalExtensions
    {
        public static IOptional<T> OrElse<T>(this IOptional<T> @this, Func<IOptional<T>> alternative)
        {
            return @this.HasValue ? @this : alternative();
        }

        public static IOptional<T> If<T>(this IOptional<T> @this, Func<T, bool> filter)
        {
            return @this.HasValue && filter(@this.Value) ? @this : Optional<T>.None;
        }
    }
}
