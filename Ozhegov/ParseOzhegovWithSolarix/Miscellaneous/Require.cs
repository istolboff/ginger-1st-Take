using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    public static class Require
    {
        public static void NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void NotNullOrWhitespace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("String parameter is null, empty or contains only whitespaces",  parameterName);
            }
        }
    }
}
