using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class Verify
    {
        public static void That(bool condition, Func<string> buildExceptionMessage)
        {
            if (!condition)
            {
                throw new InvalidOperationException(buildExceptionMessage());
            }
        }
    }
}
