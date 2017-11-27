using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class OptionalExtensions
    {
        public static IOptional<T> OrElse<T>(this IOptional<T> @this, Func<IOptional<T>> alternative)
        {
            return @this.HasValue ? @this : alternative();
        }
    }
}
