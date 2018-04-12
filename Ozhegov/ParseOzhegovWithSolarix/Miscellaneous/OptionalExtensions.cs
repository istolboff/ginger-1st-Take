using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class OptionalExtensions
    {
        public static IOptional<TResult> Map<T, TResult>(this IOptional<T> @this, Func<T, TResult> convertValue)
        {
            return @this.HasValue ? Optional.Some(convertValue(@this.Value)) : Optional.None<TResult>();
        }

        public static IOptional<T> OrElse<T>(this IOptional<T> @this, Func<IOptional<T>> alternative)
        {
            return @this.HasValue ? @this : alternative();
        }

        public static TResult Fold<T, TResult>(this IOptional<T> @this, Func<T, TResult> convert, Func<TResult> getDefaultValue)
        {
            return @this.HasValue ? convert(@this.Value) : getDefaultValue();
        }

        public static IOptional<T> If<T>(this IOptional<T> @this, Func<T, bool> filter)
        {
            return @this.HasValue && filter(@this.Value) ? @this : Optional.None<T>();
        }
    }
}
