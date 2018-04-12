using System.Collections.Generic;
using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal static class Sentence
    {
        public static IDictionary<ISentenceElementMatcher<object>, ElementMatchingResult> MatchingResults => MatchingResultsStorage;

        public static ISentenceElementMatcher<SentenceElementMatcher<TGrammarCharacteristics>> Root<TGrammarCharacteristics>(object expectedProperties)
            where TGrammarCharacteristics : GrammarCharacteristics
        {
            return new SentenceElementMatcher<TGrammarCharacteristics>(_ => _, expectedProperties);
        }

        public static PartOfSpeechMatcher Root(PartOfSpeech partOfSpeech, string content)
        {
            return new PartOfSpeechMatcher(_ => _, partOfSpeech, content);
        }

        private static readonly IDictionary<ISentenceElementMatcher<object>, ElementMatchingResult> MatchingResultsStorage =
            new Dictionary<ISentenceElementMatcher<object>, ElementMatchingResult>();
    }
}
