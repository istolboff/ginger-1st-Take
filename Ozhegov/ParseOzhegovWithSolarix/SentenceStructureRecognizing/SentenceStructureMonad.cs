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
                            return Optional.None<TResult>();
                        }

                        var intermediateMatcher = ExecuteMatcherSelector(selector, firstMatch.Value);
                        var intermediateMatch = intermediateMatcher.Match(rootSentence);
                        return intermediateMatch.HasValue
                            ? Optional.Some(projector(firstMatch.Value, intermediateMatch.Value)) 
                            : Optional.None<TResult>();
                    });
        }

        public static ISentenceElementMatcher<TResult> Select<TInput, TResult>(
            this ISentenceElementMatcher<TInput> @this,
            Func<TInput, TResult> selector)
        {
            return new FunctorBasedMatcher<TResult>(rootSentence => @this.Match(rootSentence).Map(selector));
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