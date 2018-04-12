using System;
using System.Collections.Generic;

namespace Gari.Tests.Utilities
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<TResult> PartitionToPairs<TSource, TResult>(
            this IEnumerable<TSource> @this,
            Func<TSource, TSource, TResult> makePartition)
        {
            var first = default(TSource);
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

        public static VerboseIndexer<TKey, TValue> WithVerboseIndexing<TKey, TValue>(this IDictionary<TKey, TValue> @this, string dictionaryName)
        {
            return new VerboseIndexer<TKey, TValue>(dictionaryName, @this);
        }
    }
}
