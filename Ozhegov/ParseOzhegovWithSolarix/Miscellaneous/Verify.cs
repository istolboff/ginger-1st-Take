﻿using System;
using System.Diagnostics.Contracts;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class Verify
    {
        [Pure]
        public static void That(bool condition, Func<string> buildExceptionMessage)
        {
            if (!condition)
            {
                throw new InvalidOperationException(buildExceptionMessage());
            }
        }
    }
}
