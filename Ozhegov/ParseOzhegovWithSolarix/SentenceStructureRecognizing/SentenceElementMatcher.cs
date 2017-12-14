using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal sealed class SentenceElementMatcher<TGrammarCharacteristics> : SentenceElementMatcherBase<SentenceElementMatcher<TGrammarCharacteristics>>
        where TGrammarCharacteristics : GrammarCharacteristics
    {
        public SentenceElementMatcher(
            Func<SentenceElement, SentenceElement> getElementToMatch,
            object expectedProperties)
            : base(getElementToMatch)
        {
            _expectedContent = GetExpectedValueByName(expectedProperties, nameof(Content));
            _expectedLemma = GetExpectedValueByName(expectedProperties, nameof(Lemma));
            _matchGrammarCharacteristics = BuildGrammarCharacteristicsMatcher(expectedProperties);
        }

        public string Content => Sentence.MatchingResults[this].Content;

        public string Lemma => Sentence.MatchingResults[this].LemmaVersion.Lemma;

        public TGrammarCharacteristics Detected => (TGrammarCharacteristics)Sentence.MatchingResults[this].LemmaVersion.Characteristics;

        protected override LemmaVersion MatchCore(SentenceElement elementToMatch) => 
            _expectedContent != null && !_expectedContent.Equals(elementToMatch.Content, StringComparison.OrdinalIgnoreCase)
                ? null
                : elementToMatch.LemmaVersions
                    .Where(lemmaVersion => lemmaVersion.Characteristics is TGrammarCharacteristics)
                    .FirstOrDefault(lemmaVersion => 
                        _matchGrammarCharacteristics((TGrammarCharacteristics)lemmaVersion.Characteristics) && 
                        (_expectedLemma == null || _expectedLemma.Equals(lemmaVersion.Lemma, StringComparison.OrdinalIgnoreCase)));

        protected override SentenceElementMatcher<TGrammarCharacteristics> This => this; 

        private static string GetExpectedValueByName(object expectedProperties, string valueName) => 
            expectedProperties.GetType().GetProperty(valueName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(expectedProperties) as string;

        private static Predicate<TGrammarCharacteristics> BuildGrammarCharacteristicsMatcher(object expectedProperties)
        {
            var expectedPropertiesInfo = expectedProperties
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => !property.Name.IsOneOf(nameof(Content), nameof(Lemma)))
                .ToArray();

            if (!expectedPropertiesInfo.Any())
            {
                return grammarCharacteristics => true;
            }

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

            if (expectedPropertiesInfo.Any(property => property.PropertyType.RemoveNullabilityIfAny().IsOneOf(typeof(Form), typeof(Gender))))
            {
                issues.Add($"A property of type 'Одушевленность' or 'Пол' is used when matching a grammar characteristics. Are you sure you're doing the right thing?");
            }

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
                    AppropriateType = appropriatePropertiesInfo.Single(ap => ap.Name == property.Name).PropertyType.RemoveNullabilityIfAny()
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
        private readonly string _expectedLemma;
        private readonly Predicate<TGrammarCharacteristics> _matchGrammarCharacteristics;
    }
}
