using System.Collections.Generic;

namespace ParseOzhegovWithSolarix.Solarix
{
    public interface IRussianGrammarParser
    {
        IReadOnlyCollection<SentenceElement> Parse(string text);
    }
}