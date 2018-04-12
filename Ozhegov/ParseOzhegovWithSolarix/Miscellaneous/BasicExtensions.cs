using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class BasicExtensions
    {
        public static T If<T>(this T value, Func<T, bool> filter) 
        {
            return filter(value) ? value : default(T);
        }

        public static bool IsOneOf<T>(this T value, T firstValue, T secondValue, params T[] allOtherValues)
        {
            return value.Equals(firstValue) ||
                   value.Equals(secondValue) ||
                   (allOtherValues != null && Array.IndexOf(allOtherValues, value) >= 0);
        }

        public static TResult Apply<T, TResult>(this T @this, Func<T, TResult> functor)
        {
            return functor(@this);
        }
    }
}
