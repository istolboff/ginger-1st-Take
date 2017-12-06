using System;
using System.Collections.Generic;
using System.Linq;

namespace Gari.Tests.Utilities
{
    internal static class InputStringVariationGenerator
    {
        public static IEnumerable<string> GenerateVariations(string value)
        {
            return from alternativesRealization in GenerateAlternativesCore(value)
                   from singleVariantion in GenerateVariationsCore(alternativesRealization)
                   select singleVariantion;
        }

        private static IEnumerable<string> GenerateAlternativesCore(string value)
        {
            var alternativesSpecifications = value
                        .Select((ch, index) => new { ch, index })
                        .Where(item => item.ch == '(')
                        .Select(item =>
                                    new
                                    {
                                        startIndex = item.index,
                                        endIndex = value.IndexOf(')', item.index + 1)
                                    })
                        .Select(item => 
                                    value
                                        .Substring(item.startIndex + 1, item.endIndex - item.startIndex - 1)
                                        .Split('|')
                                        .Select(alternative => new { item.startIndex, item.endIndex, alternative })
                                        .ToArray())
                        .ToArray();

            return Transpose(alternativesSpecifications)
                        .Select(alternativesCombination =>
                                alternativesCombination
                                    .Reverse()
                                    .Aggregate(
                                        value,
                                        (currentValue, singleAlternative) =>
                                            ReplaceRangeWith(
                                                currentValue,
                                                singleAlternative.startIndex,
                                                singleAlternative.endIndex - singleAlternative.startIndex + 1,
                                                singleAlternative.alternative)));
        }

        private static IEnumerable<string> GenerateVariationsCore(string value)
        {
            var optionalElements = value
                        .Select((ch, index) => new { ch, index, followsSpace = index > 0 && value[index - 1] == ' ' })
                        .Where(item => item.ch == '[')
                        .Select(item =>
                                    new
                                    {
                                        item.followsSpace,
                                        startIndex = item.index,
                                        optionalText = value.Substring(item.index + 1, value.IndexOf(']', item.index + 1) - item.index - 1)
                                    })
                        .ToArray();

            return
                from bitMask in Enumerable.Range(0, (int)Math.Pow(2, optionalElements.Length))
                let result = optionalElements
                                .Select((element, index) => new
                                {
                                    index,
                                    startIndex = element.followsSpace ? element.startIndex - 1 : element.startIndex,
                                    charsToRemove = element.optionalText.Length + (element.followsSpace ? 3 : 2),
                                    textToInsert = (element.followsSpace ? " " : string.Empty) + element.optionalText
                                })
                                .Reverse()
                                .Aggregate(
                                    value,
                                    (currentValue, optionalElement) =>
                                        ReplaceRangeWith(
                                            currentValue,
                                            optionalElement.startIndex, 
                                            optionalElement.charsToRemove,
                                            ((int)Math.Pow(2, optionalElement.index) & bitMask) != 0
                                                ? optionalElement.textToInsert
                                                : string.Empty))
                select !value.StartsWith(" ") && result.StartsWith(" ") ? result.Substring(1) : result;
        }

        private static string ReplaceRangeWith(string value, int rangeStart, int rangeLength, string replaceWithThis)
        {
            return value.Remove(rangeStart, rangeLength).Insert(rangeStart, replaceWithThis);
        }

        private static IEnumerable<IEnumerable<T>> Transpose<T>(IEnumerable<IEnumerable<T>> sequence)
        {
            using (var sequenceEnumerator = sequence.GetEnumerator())
            {
                return TransposeCore(sequenceEnumerator);
            }
        }

        private static IEnumerable<IEnumerable<T>> TransposeCore<T>(IEnumerator<IEnumerable<T>> sequenceEnumerator)
        {
            if (!sequenceEnumerator.MoveNext())
            {
                yield return new List<T>();
            }
            else
            {
                var currentElements = sequenceEnumerator.Current;
                foreach (var result in from tailSequence in TransposeCore(sequenceEnumerator)
                                       from element in currentElements
                                       select new[] { element }.Concat(tailSequence))
                {
                    yield return result;
                }
            }
        }
    }
}