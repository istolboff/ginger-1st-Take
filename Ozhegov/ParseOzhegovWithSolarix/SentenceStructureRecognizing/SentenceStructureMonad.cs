using System;
using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal static class SentenceStructureMonad
    {
        public static ISentenceElementMatcher<TResult> SelectMany<TInput, TIntermediate, TResult>(
            this ISentenceElementMatcher<TInput> @this,
            Func<TInput, ISentenceElementMatcher<TIntermediate>> selector,
            Func<TInput, TIntermediate, TResult> projector)
        {
            return new FunctorBasedMatcher<TResult>(
                    rootSentence =>
                    {
                        var firstMatch = @this.Match(rootSentence);
                        if (!firstMatch.HasValue)
                        {
                            return Optional<TResult>.None;
                        }

                        var intermediateMatcher = selector(firstMatch.Value);
                        var intermediateMatch = intermediateMatcher.Match(rootSentence);
                        return intermediateMatch.HasValue
                            ? new Optional<TResult>(projector(firstMatch.Value, intermediateMatch.Value)) 
                            : Optional<TResult>.None;
                    });
        }
    }
}
