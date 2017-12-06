using System;
using System.Collections.Generic;
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

                        var intermediateMatcher = ExecuteMatcherSelector(selector, firstMatch.Value);
                        var intermediateMatch = intermediateMatcher.Match(rootSentence);
                        return intermediateMatch.HasValue
                            ? new Optional<TResult>(projector(firstMatch.Value, intermediateMatch.Value)) 
                            : Optional<TResult>.None;
                    });
        }

        public static ISentenceElementMatcher<TResult> Select<TInput, TResult>(
            this ISentenceElementMatcher<TInput> @this,
            Func<TInput, TResult> selector)
        {
            return new FunctorBasedMatcher<TResult>(
                    rootSentence =>
                    {
                        var match = @this.Match(rootSentence);
                        return match.HasValue ? new Optional<TResult>(selector(match.Value)) : Optional<TResult>.None;
                    });
        }

        public static ISentenceElementMatcher<TResult> Where<TResult>(
            this ISentenceElementMatcher<TResult> @this, 
            Func<TResult, bool> filter)
        {
            return new FunctorBasedMatcher<TResult>(
                    rootSentence =>
                    {
                        return @this.Match(rootSentence).If(filter);
                    });
        }

        private static ISentenceElementMatcher<TIntermediate> ExecuteMatcherSelector<TInput, TIntermediate>(
            Func<TInput, ISentenceElementMatcher<TIntermediate>> selector,
            TInput firstMatchValue)
        {
            var selectorInvokationKey = new KeyValuePair<object, object>(selector, firstMatchValue);
            if (MemoizedSelectors.TryGetValue(selectorInvokationKey, out var selectorResult))
            {
                return (ISentenceElementMatcher<TIntermediate>)selectorResult;
            }

            var result = selector(firstMatchValue);
            MemoizedSelectors.Add(selectorInvokationKey, result);
            return result;
        }

        private static readonly IDictionary<KeyValuePair<object, object>, object> MemoizedSelectors = new Dictionary<KeyValuePair<object, object>, object>();
    }
}