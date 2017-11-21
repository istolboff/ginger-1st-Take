﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal static class Sentence
    {
        public static SentenceStructureMonad<SentenceElementMatcher<TGrammarCharacteristics>> Root<TGrammarCharacteristics>(object expectedProperties)
            where TGrammarCharacteristics : GrammarCharacteristics
        {
            return new SentenceStructureMonad<SentenceElementMatcher<TGrammarCharacteristics>>(
                new SentenceElementMatcher<TGrammarCharacteristics>(
                    _ => _,
                    expectedProperties, 
                    new Dictionary<ISentenceElementMatcher, ElementMatchingResult>()));
        }
    }

    internal interface ISentenceElementMatcher
    {
        bool Match(SentenceElement rootSentenceElement);
    }

    internal sealed class SentenceElementMatcher<TGrammarCharacteristics> : ISentenceElementMatcher
        where TGrammarCharacteristics : GrammarCharacteristics
    {
        public SentenceElementMatcher(
            Func<SentenceElement, SentenceElement> getElementToMatch,
            object expectedProperties,
            IDictionary<ISentenceElementMatcher, ElementMatchingResult> matchingResults)
        {
            _getElementToMatch = getElementToMatch;
            _expectedContent = GetExpectedContent(expectedProperties);
            _matchGrammarCharacteristics = BuildGrammarCharacteristicsMatcher(expectedProperties);
            _matchingResults = matchingResults;
        }

        public string Content
        {
            get
            {
                return _matchingResults[this].Content;
            }
        }

        public string Lemma
        {
            get
            {
                return _matchingResults[this].LemmaVersion.Lemma;
            }
        }

        public TGrammarCharacteristics Detected
        {
            get
            {
                return (TGrammarCharacteristics)_matchingResults[this].LemmaVersion.Characteristics;
            }
        }

        public SentenceStructureMonad<SentenceElementMatcher<TChildGrammarCharacteristics>> Subject<TChildGrammarCharacteristics>(object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            return new SentenceStructureMonad<SentenceElementMatcher<TChildGrammarCharacteristics>>(
                AddChildElementMatcher<TChildGrammarCharacteristics>(LinkType.SUBJECT_link, expectedProperties));
        }

        public bool Match(SentenceElement rootSentenceElement)
        {
            var elementToMatch = _getElementToMatch(rootSentenceElement);
            if (elementToMatch == null)
            {
                return false;
            }

            if (_expectedContent != null && _expectedContent != elementToMatch.Content)
            {
                return false;
            }

            var matchingLemma = elementToMatch.LemmaVersions
                .Where(lemmaVersion => lemmaVersion.Characteristics is TGrammarCharacteristics)
                .FirstOrDefault(lemmaVersion => _matchGrammarCharacteristics((TGrammarCharacteristics)lemmaVersion.Characteristics));

            if (matchingLemma == null)
            {
                return false;
            }

            _matchingResults.Add(this, new ElementMatchingResult(elementToMatch.Content, matchingLemma));

            return _childElementMatchers.All(matcher => matcher.Match(rootSentenceElement));
        }

        private SentenceElementMatcher<TChildGrammarCharacteristics> AddChildElementMatcher<TChildGrammarCharacteristics>(
            LinkType expectedLinkType,
            object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            var nextChildIndex = _childElementMatchers.Count;
            var result = new SentenceElementMatcher<TChildGrammarCharacteristics>(
                rootElement =>
                    _getElementToMatch(rootElement)
                        ?.Children?[nextChildIndex]
                        ?.If(child => child.LeafLinkType == expectedLinkType),
                expectedProperties,
                _matchingResults);
            _childElementMatchers.Add(result);
            return result;
        }

        private static string GetExpectedContent(object expectedProperties)
        {
            return expectedProperties.GetType().GetProperty(ContentPropertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(expectedProperties) as string;
        }

        private static Predicate<TGrammarCharacteristics> BuildGrammarCharacteristicsMatcher(object expectedProperties)
        {
            var expectedPropertiesInfo = expectedProperties
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.Name != ContentPropertyName)
                .ToArray();

            var appropriatePropertiesInfo = typeof(TGrammarCharacteristics).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            CheckExpectedPropertiesCorrectness(expectedPropertiesInfo, appropriatePropertiesInfo);

            var propertyValueComparers = expectedPropertiesInfo
                .Select(property => new
                {
                    ExpectedValue = property.GetValue(expectedProperties),
                    GrammarCharacteristicProperty = appropriatePropertiesInfo.Single(apc => apc.Name == property.Name)
                })
                .ToArray();

            return grammarCharacteristics => 
                propertyValueComparers.All(comparer => 
                    comparer.ExpectedValue.Equals(comparer.GrammarCharacteristicProperty.GetValue(grammarCharacteristics)));
        }

        private static void CheckExpectedPropertiesCorrectness(PropertyInfo[] expectedPropertiesInfo, PropertyInfo[] appropriatePropertiesInfo)
        {
            var issues = new List<string>();

            var propertiesWithUnexpectedNames = expectedPropertiesInfo
                .Where(property => appropriatePropertiesInfo.All(ap => ap.Name != property.Name))
                .Select(property => property.Name)
                .AsImmutable();

            if (propertiesWithUnexpectedNames.Any())
            {
                issues.Add(
                    $"Properties with names {string.Join(", ", propertiesWithUnexpectedNames)} are not valid when matching {typeof(TGrammarCharacteristics).Name}");
            }

            var propertiesWithInvalidTypes = expectedPropertiesInfo
                .Where(property => appropriatePropertiesInfo.Any(ap => ap.Name == property.Name))
                .Select(property => new
                {
                    property.Name,
                    ProvidedType = property.PropertyType,
                    AppropriateType = appropriatePropertiesInfo.Single(ap => ap.Name == property.Name).PropertyType
                })
                .Where(item => item.AppropriateType != item.ProvidedType)
                .AsImmutable();

            if (propertiesWithInvalidTypes.Any())
            {
                issues.Add(
                    $"Properties {string.Join(", ", propertiesWithInvalidTypes.Select(p => p.Name))} have invalid types when matching {typeof(TGrammarCharacteristics).Name}.");
            }

            Verify.That(!issues.Any(), () => string.Join(Environment.NewLine, issues));
        }

        private readonly Func<SentenceElement, SentenceElement> _getElementToMatch;
        private readonly string _expectedContent;
        private readonly Predicate<TGrammarCharacteristics> _matchGrammarCharacteristics;
        private readonly IDictionary<ISentenceElementMatcher, ElementMatchingResult> _matchingResults;
        private readonly List<ISentenceElementMatcher> _childElementMatchers = new List<ISentenceElementMatcher>();

        private const string ContentPropertyName = "Content";
    }

    internal struct ElementMatchingResult
    {
        public ElementMatchingResult(string content, LemmaVersion lemmaVersion)
        {
            Content = content;
            LemmaVersion = lemmaVersion;
        }

        public string Content { get; }

        public LemmaVersion LemmaVersion { get; }
    }

    internal sealed class SentenceStructureMonad<TMatcher>
        where TMatcher : ISentenceElementMatcher
    {
        public SentenceStructureMonad(TMatcher matcher)
        {
            _matcher = matcher;
        }

        public Func<SentenceElement, Optional<TResult>> SelectMany<TIntermediateMatcher, TResult>(
            Func<TMatcher, SentenceStructureMonad<TIntermediateMatcher>> selector,
            Func<TMatcher, TIntermediateMatcher, TResult> projector)
            where TIntermediateMatcher : ISentenceElementMatcher
        {
            return rootSentence =>
            {
                if (!_matcher.Match(rootSentence))
                {
                    return Optional<TResult>.None;
                }

                var intermediateMatcher = selector(_matcher);
                return intermediateMatcher._matcher.Match(rootSentence) 
                    ? new Optional<TResult>(projector(_matcher, intermediateMatcher._matcher)) 
                    : Optional<TResult>.None;
            };
        }

        private readonly TMatcher _matcher;
    }
}
