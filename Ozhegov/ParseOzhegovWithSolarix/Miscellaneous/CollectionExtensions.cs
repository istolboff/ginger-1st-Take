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
    }
}
