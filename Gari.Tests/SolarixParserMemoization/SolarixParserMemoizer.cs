using System.Collections.Generic;
using ParseOzhegovWithSolarix.Solarix;
using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix.SolarixParserMemoization
{
    public sealed class SolarixParserMemoizer : IRussianGrammarParser
    {
        public SolarixParserMemoizer(IRussianGrammarParser wrappedParser)
        {
            _wrappedParser = wrappedParser;
            EnsureDatabaseCreated();
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

        private IOptional<IReadOnlyCollection<SentenceElement>> TryToRecallResult(string text)
        {

        }

        private void Memoize(string text, IReadOnlyCollection<SentenceElement> result)
        {

        }

        private static EnsureDatabaseCreated()
        {

        }

        private readonly IRussianGrammarParser _wrappedParser;
    }
}
