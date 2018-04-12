﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gari.Tests.Utilities
{
    public sealed class SolarixParserMemoizer : IRussianGrammarParser
    {
        static SolarixParserMemoizer()
        {
            JsonConvert.DefaultSettings = () => 
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            };
        }

        public SolarixParserMemoizer(IRussianGrammarParser wrappedParser)
        {
            _wrappedParser = wrappedParser;
            EnsureDataFileExists();
            _knownSentences = LoadKnownSentences();
        }

        public IReadOnlyCollection<SentenceElement> Parse(string text)
        {
            var memoizedResult = TryToRecallResult(text);
            if (memoizedResult.HasValue)
            {
                return memoizedResult.Value;
            }

            var result = _wrappedParser.Parse(text);
            Memoize(text, result);
            return result;
        }

        public void Dispose()
        {
            _wrappedParser.Dispose();
        }

        private IOptional<IReadOnlyCollection<SentenceElement>> TryToRecallResult(string text)
        {
            return _knownSentences.TryGetValue(text, out var sentenceElements) 
                        ? Optional.Some(sentenceElements.Value) 
                        : Optional.None<IReadOnlyCollection<SentenceElement>>();
        }

        private void Memoize(string text, IReadOnlyCollection<SentenceElement> result)
        {
            _knownSentences.Add(text, new Lazy<IReadOnlyCollection<SentenceElement>>(() => result));
            File.AppendAllLines(DataFilePath, new[] { text, Serialize(result) }, Encoding.UTF8);
        }

        private static string DataFilePath => 
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SolarixRussianGrammarParser.memoized");

        private static void EnsureDataFileExists()
        {
            if (!File.Exists(DataFilePath))
            {
                File.Create(DataFilePath).Dispose();
            }
        }

        private static IDictionary<string, Lazy<IReadOnlyCollection<SentenceElement>>> LoadKnownSentences()
        {
            return File
                .ReadLines(DataFilePath, Encoding.UTF8)
                .PartitionToPairs((text, serializedData) => new { text, serializedData })
                .ToDictionary(item => item.text, item => new Lazy<IReadOnlyCollection<SentenceElement>>(() => Deserialize(item.serializedData)));
        }

        private static string Serialize(IReadOnlyCollection<SentenceElement> sentenceElements)
        {
            return JsonConvert.SerializeObject(
                sentenceElements, 
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        private static IReadOnlyCollection<SentenceElement> Deserialize(string serializedData)
        {
            return JsonConvert.DeserializeObject<IReadOnlyCollection<SentenceElement>>(
                serializedData,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        private readonly IRussianGrammarParser _wrappedParser;
        private readonly IDictionary<string, Lazy<IReadOnlyCollection<SentenceElement>>> _knownSentences;
    }
}
