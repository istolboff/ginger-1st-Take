using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParseOzhegovWithSolarix.Miscellaneous;
using System.Collections.Generic;

namespace Gari.Tests.Utilities
{
    internal sealed class VerboseIndexer<TKey, TValue>
    {
        public VerboseIndexer(string dictionaryName, IDictionary<TKey, TValue> dictionary)
        {
            Require.NotNullOrWhitespace(dictionaryName, nameof(dictionaryName));
            Require.NotNull(dictionary, nameof(dictionary));

            _dictionaryName = dictionaryName;
            _dictionary = dictionary;
        }

        public TValue this[TKey key]
        {
            get
            {
                var keyExists = _dictionary.TryGetValue(key, out TValue result);
                Assert.IsTrue(
                    keyExists,
                    $"Dictionary {_dictionaryName} does not contain key '{key}'. " + 
                    $"Only following keys are present: {{ {string.Join(", ", _dictionary.Keys)} }}.");

                return result;
            }
        }

        private readonly string _dictionaryName;
        private readonly IDictionary<TKey, TValue> _dictionary;
    }
}
