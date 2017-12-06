using System;
using System.Collections.Generic;

namespace ParseOzhegovWithSolarix.Solarix
{
    public interface IRussianGrammarParser : IDisposable 
    {
        IReadOnlyCollection<SentenceElement> Parse(string text);
    }
}