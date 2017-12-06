using System;
using System.Collections.Generic;
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

        public static IEnumerable<TResult> Partition<TSource, TResult>(
            this IEnumerable<TSource> @this, 
            Func<TSource, TSource, TResult> makePartition)
        {
            TSource first = default(TSource);
            var setFirstElement = true;

            foreach (var item in @this)
            {
                if (setFirstElement)
                {
                    first = item;
                    setFirstElement = false;
                }
                else
                {
                    yield return makePartition(first, item);
                    setFirstElement = true;
                }
            }
        }
    }
}
