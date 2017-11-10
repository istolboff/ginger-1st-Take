using System;
using System.Collections.Generic;
using System.Linq;

namespace Gari.Tests
{
    internal static class InputStringVariationGenerator
    {
        internal static IEnumerable<string> GenerateVariations(string value)
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
                                        currentValue
                                            .Remove(optionalElement.startIndex, optionalElement.charsToRemove)
                                            .Insert(
                                                optionalElement.startIndex,
                                                ((int)Math.Pow(2, optionalElement.index) & bitMask) != 0
                                                    ? optionalElement.textToInsert
                                                    : string.Empty))
                select !value.StartsWith(" ") && result.StartsWith(" ") ? result.Substring(1) : result;
        }
    }
}