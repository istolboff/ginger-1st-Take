﻿using System;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal abstract class SentenceElementMatcherBase<TResult> : ISentenceElementMatcher<TResult> 
        where TResult : SentenceElementMatcherBase<TResult>
    {
        protected SentenceElementMatcherBase(Func<SentenceElement, SentenceElement> getElementToMatch)
        {
            _getElementToMatch = getElementToMatch;
        }

        public SentenceElementMatcher<TChildGrammarCharacteristics> Subject<TChildGrammarCharacteristics>(object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            return AddChildElementMatcher<TChildGrammarCharacteristics>(LinkType.SUBJECT_link, expectedProperties);
        }

        public SentenceElementMatcher<TChildGrammarCharacteristics> Object<TChildGrammarCharacteristics>(object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            return AddChildElementMatcher<TChildGrammarCharacteristics>(LinkType.OBJECT_link, expectedProperties);
        }

        public SentenceElementMatcher<TChildGrammarCharacteristics> Rhema<TChildGrammarCharacteristics>(object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            return AddChildElementMatcher<TChildGrammarCharacteristics>(LinkType.RHEMA_link, expectedProperties);
        }

        public SentenceElementMatcher<TChildGrammarCharacteristics> RightGenitiveObject<TChildGrammarCharacteristics>(object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            return AddChildElementMatcher<TChildGrammarCharacteristics>(LinkType.RIGHT_GENITIVE_OBJECT_link, expectedProperties);
        }

        public SentenceElementMatcher<TChildGrammarCharacteristics> NextCollocationItem<TChildGrammarCharacteristics>(object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            return AddChildElementMatcher<TChildGrammarCharacteristics>(LinkType.NEXT_COLLOCATION_ITEM_link, expectedProperties);
        }

        public SentenceElementMatcher<TChildGrammarCharacteristics> Attribute<TChildGrammarCharacteristics>(object expectedProperties, bool uglyHack_DoNotIncrementNextChildIndex = false)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            return AddChildElementMatcher<TChildGrammarCharacteristics>(LinkType.ATTRIBUTE_link, expectedProperties, uglyHack_DoNotIncrementNextChildIndex);
        }

        public PartOfSpeechMatcher Object(PartOfSpeech partOfSpeech, string content)
        {
            return AddChildElementMatcher(LinkType.OBJECT_link, partOfSpeech, content);
        }

        public PartOfSpeechMatcher NextCollocationItem(PartOfSpeech partOfSpeech, string content)
        {
            return AddChildElementMatcher(LinkType.NEXT_COLLOCATION_ITEM_link, partOfSpeech, content);
        }

        public PartOfSpeechMatcher NextClause(PartOfSpeech partOfSpeech, string content)
        {
            return AddChildElementMatcher(LinkType.NEXT_CLAUSE_link, partOfSpeech, content);
        }

        public PartOfSpeechMatcher NegationParticle(PartOfSpeech partOfSpeech, string content)
        {
            return AddChildElementMatcher(LinkType.NEGATION_PARTICLE_link, partOfSpeech, content);
        }

        public PartOfSpeechMatcher Attribute(PartOfSpeech partOfSpeech, string content)
        {
            return AddChildElementMatcher(LinkType.ATTRIBUTE_link, partOfSpeech, content);
        }

        public PartOfSpeechMatcher PrefixParticle(PartOfSpeech partOfSpeech, string content)
        {
            return AddChildElementMatcher(LinkType.PREFIX_PARTICLE_link, partOfSpeech, content);
        }

        public IOptional<TResult> Match(SentenceElement rootSentenceElement)
        {
            var elementToMatch = _getElementToMatch(rootSentenceElement);
            if (elementToMatch == null)
            {
                return Optional<TResult>.None;
            }

            var matchingLemma = MatchCore(elementToMatch);
            if (matchingLemma == null)
            {
                return Optional<TResult>.None;
            }

            Sentence.MatchingResults.Add(this, new ElementMatchingResult(elementToMatch.Content, matchingLemma));

            return new Optional<TResult>(This);
        }

        public static ISentenceElementMatcher<TResult> operator |(
            SentenceElementMatcherBase<TResult> left, 
            SentenceElementMatcherBase<TResult> right)
        {
            return new FunctorBasedMatcher<TResult>(
                sentenceElement => left.Match(sentenceElement).OrElse(() => right.Match(sentenceElement)));
        }

        protected abstract LemmaVersion MatchCore(SentenceElement elementToMatch);

        protected abstract TResult This { get; }

        private SentenceElementMatcher<TChildGrammarCharacteristics> AddChildElementMatcher<TChildGrammarCharacteristics>(
            LinkType expectedLinkType,
            object expectedProperties,
            bool uglyHack_DoNotIncrementNextChildIndex = false)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            var currentChildIndex = _nextChildIndex;
            if (!uglyHack_DoNotIncrementNextChildIndex)
            {
                ++_nextChildIndex;
            }

            return new SentenceElementMatcher<TChildGrammarCharacteristics>(
                rootElement =>
                    _getElementToMatch(rootElement)
                        ?.Children
                        ?.TryGetAt(currentChildIndex)
                        ?.If(child => child.LeafLinkType == expectedLinkType),
                expectedProperties);
        }

        private PartOfSpeechMatcher AddChildElementMatcher(
            LinkType expectedLinkType, 
            PartOfSpeech partOfSpeech, 
            string content)
        {
            var currentChildIndex = _nextChildIndex++;
            return new PartOfSpeechMatcher(
                rootElement =>
                    _getElementToMatch(rootElement)
                        ?.Children
                        ?.TryGetAt(currentChildIndex)
                        ?.If(child => child.LeafLinkType == expectedLinkType),
                partOfSpeech, 
                content);
        }

        private readonly Func<SentenceElement, SentenceElement> _getElementToMatch;
        private int _nextChildIndex;
    }
}
