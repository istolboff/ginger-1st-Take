﻿using System.Collections.Generic;
using System.Linq;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    public static class CollectionExtensions
    {
        public static IReadOnlyCollection<T> AsImmutable<T>(this IEnumerable<T> @this)
        {
            return (@this as IReadOnlyCollection<T>) ?? @this.ToList();
        }

        public static T TryGetAt<T>(this IList<T> @this, int index)
        {
            return index < @this.Count ? @this[index] : default(T);
        }
    }
}
