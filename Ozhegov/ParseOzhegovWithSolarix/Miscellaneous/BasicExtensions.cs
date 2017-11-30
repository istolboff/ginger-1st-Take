using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class BasicExtensions
    {
        public static T If<T>(this T value, Func<T, bool> filter) 
        {
            return filter(value) ? value : default(T);
        }
    }
}
