using System;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal sealed class FunctorBasedMatcher<TResult> : ISentenceElementMatcher<TResult>
    {
        public FunctorBasedMatcher(Func<SentenceElement, IOptional<TResult>> matchElement)
        {
            _matchElement = matchElement;
        }

        public IOptional<TResult> Match(SentenceElement rootSentenceElement)
        {
            return _matchElement(rootSentenceElement);
        }

        private readonly Func<SentenceElement, IOptional<TResult>> _matchElement;
    }
}
