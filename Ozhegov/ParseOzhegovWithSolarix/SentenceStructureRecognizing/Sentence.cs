using System.Collections.Generic;
using ParseOzhegovWithSolarix.Solarix;
using System.Threading;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal static class Sentence
    {
        public static IDictionary<ISentenceElementMatcher<object>, ElementMatchingResult> MatchingResults => MatchingResultsStorage.Value;

        public static ISentenceElementMatcher<SentenceElementMatcher<TGrammarCharacteristics>> Root<TGrammarCharacteristics>(object expectedProperties)
            where TGrammarCharacteristics : GrammarCharacteristics
        {
            return new SentenceElementMatcher<TGrammarCharacteristics>(_ => _, expectedProperties);
        }

        public static PartOfSpeechMatcher Root(PartOfSpeech partOfSpeech, string content)
        {
            return new PartOfSpeechMatcher(_ => _, partOfSpeech, content);
        }

        private static readonly ThreadLocal<Dictionary<ISentenceElementMatcher<object>, ElementMatchingResult>> MatchingResultsStorage =
            new ThreadLocal<Dictionary<ISentenceElementMatcher<object>, ElementMatchingResult>>(
                   () => new Dictionary<ISentenceElementMatcher<object>, ElementMatchingResult>());
    }
}
