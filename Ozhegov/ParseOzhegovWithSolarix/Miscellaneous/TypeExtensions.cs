using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class TypeExtensions
    {
        public static Type RemoveNullabilityIfAny(this Type @this)
        {
            return Nullable.GetUnderlyingType(@this) ?? @this;
        }
    }
}
