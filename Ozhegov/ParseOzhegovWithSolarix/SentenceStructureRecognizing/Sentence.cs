using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;
using System.Threading;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal static class Sentence
    {
        public static IDictionary<ISentenceElementMatcher<object>, ElementMatchingResult> MatchingResults
        {
            get
            {
                return MatchingResultsStorage.Value;
            }
        }

        public static ISentenceElementMatcher<SentenceElementMatcher<TGrammarCharacteristics>> Root<TGrammarCharacteristics>(object expectedProperties)
            where TGrammarCharacteristics : GrammarCharacteristics
        {
            return new SentenceElementMatcher<TGrammarCharacteristics>(_ => _, expectedProperties);
        }

        public static PartOfSpeechMatcher Root(PartOfSpeech partOfSpeech, string content)
        {
            return new PartOfSpeechMatcher(_ => _, partOfSpeech, content);
        }

        private static readonly ThreadLocal<Dictionary<ISentenceElementMatcher<object>, ElementMatchingResult>> MatchingResultsStorage =
            new ThreadLocal<Dictionary<ISentenceElementMatcher<object>, ElementMatchingResult>>(
                   () => new Dictionary<ISentenceElementMatcher<object>, ElementMatchingResult>());
    }

    internal interface ISentenceElementMatcher<out TResult>
    {
        IOptional<TResult> Match(SentenceElement rootSentenceElement);
    }

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

        public PartOfSpeechMatcher NextCollocationItem(PartOfSpeech partOfSpeech, string content)
        {
            return AddChildElementMatcher(LinkType.NEXT_COLLOCATION_ITEM_link, partOfSpeech, content);
        }

        public SentenceElementMatcher<TChildGrammarCharacteristics> Rhema<TChildGrammarCharacteristics>(object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            return AddChildElementMatcher<TChildGrammarCharacteristics>(LinkType.RHEMA_link, expectedProperties);
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

        public abstract LemmaVersion MatchCore(SentenceElement elementToMatch);

        protected abstract TResult This { get; }

        private SentenceElementMatcher<TChildGrammarCharacteristics> AddChildElementMatcher<TChildGrammarCharacteristics>(
            LinkType expectedLinkType,
            object expectedProperties)
            where TChildGrammarCharacteristics : GrammarCharacteristics
        {
            var currentChildIndex = _nextChildIndex++;
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

    internal sealed class SentenceElementMatcher<TGrammarCharacteristics> : SentenceElementMatcherBase<SentenceElementMatcher<TGrammarCharacteristics>>
        where TGrammarCharacteristics : GrammarCharacteristics
    {
        public SentenceElementMatcher(
            Func<SentenceElement, SentenceElement> getElementToMatch,
            object expectedProperties)
            : base(getElementToMatch)
        {
            _expectedContent = GetExpectedContent(expectedProperties);
            _matchGrammarCharacteristics = BuildGrammarCharacteristicsMatcher(expectedProperties);
        }

        public string Content
        {
            get
            {
                return Sentence.MatchingResults[this].Content;
            }
        }

        public string Lemma
        {
            get
            {
                return Sentence.MatchingResults[this].LemmaVersion.Lemma;
            }
        }

        public TGrammarCharacteristics Detected
        {
            get
            {
                return (TGrammarCharacteristics)Sentence.MatchingResults[this].LemmaVersion.Characteristics;
            }
        }

        public override LemmaVersion MatchCore(SentenceElement elementToMatch)
        {
            return _expectedContent != null && _expectedContent != elementToMatch.Content
                ? null
                : elementToMatch.LemmaVersions
                    .Where(lemmaVersion => lemmaVersion.Characteristics is TGrammarCharacteristics)
                    .FirstOrDefault(lemmaVersion => _matchGrammarCharacteristics((TGrammarCharacteristics)lemmaVersion.Characteristics));
        }

        protected override SentenceElementMatcher<TGrammarCharacteristics> This => this; 

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

        private readonly string _expectedContent;
        private readonly Predicate<TGrammarCharacteristics> _matchGrammarCharacteristics;

        private const string ContentPropertyName = "Content";
    }

    internal sealed class PartOfSpeechMatcher : SentenceElementMatcherBase<PartOfSpeechMatcher>
    {
        public PartOfSpeechMatcher(
            Func<SentenceElement, SentenceElement> getElementToMatch, 
            PartOfSpeech expectedPartOfSpeech, 
            string expectedContent)
            : base(getElementToMatch)
        {
            _expectedPartOfSpeech = expectedPartOfSpeech;
            _expectedContent = expectedContent;
        }

        public override LemmaVersion MatchCore(SentenceElement elementToMatch)
        {
            return _expectedContent != elementToMatch.Content 
                ? null
                : elementToMatch.LemmaVersions.FirstOrDefault(lemmaVersion => lemmaVersion.PartOfSpeech == _expectedPartOfSpeech);
        }

        protected override PartOfSpeechMatcher This => this; 

        private readonly PartOfSpeech _expectedPartOfSpeech;
        private readonly string _expectedContent;
    }

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
