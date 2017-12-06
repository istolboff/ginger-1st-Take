using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;

namespace Gari.Tests.Utilities
{
    public static class TemporaryParsersWritingHelper
    {
        public static void DumpParsingExpressionSkeleton(string sentenceText, SentenceElement sentenceElement)
        {
            Trace.WriteLine($"// {sentenceText}");
            DumpParsingExpressionSkeletonCore(sentenceElement, "Sentence");
            Trace.WriteLine("select new ?LogicalPredicate?(??????)");
        }

        private static void DumpParsingExpressionSkeletonCore(SentenceElement sentenceElement, string parentMatcherName)
        {
            var matcherName = GetMatcherName(sentenceElement.Content);
            Trace.WriteLine($"from {matcherName} in {parentMatcherName}.{CreateSelectingMethod(sentenceElement)}");
            foreach (var child in sentenceElement.Children)
            {
                DumpParsingExpressionSkeletonCore(child, matcherName);
            }
        }

        private static string GetMatcherName(string content)
        {
            return (PunctuationMatcherNames.TryGetValue(content, out var specialName) ? specialName : content).ToLower();
        }

        private static string CreateSelectingMethod(SentenceElement sentenceElement)
        {
            var result = new StringBuilder();
            result.Append(
                sentenceElement.LeafLinkType == null ? "Root" :
                new Dictionary<LinkType, string>
                {
                    { LinkType.SUBJECT_link, "Subject" },
                    { LinkType.OBJECT_link, "Object" },
                    { LinkType.RHEMA_link, "Rhema" },
                    { LinkType.NEXT_COLLOCATION_ITEM_link, "NextCollocationItem" },
                    { LinkType.NEGATION_PARTICLE_link, "NegationParticle" },
                    { LinkType.NEXT_CLAUSE_link, "NextClause" }
                }[sentenceElement.LeafLinkType.Value]);

            var firstLemma = sentenceElement.LemmaVersions.First();
            if (firstLemma.Characteristics is NullGrammarCharacteristics)
            {
                result.Append($"(PartOfSpeech.{firstLemma.PartOfSpeech}, \"{sentenceElement.Content}\")");
            }
            else
            {
                var characteristicsType = firstLemma.Characteristics.GetType();
                result
                    .Append("<")
                    .Append(characteristicsType.Name.Replace("Characteristics", string.Empty))
                    .Append(">(new { ");

                result.Append(
                    string.Join(
                        ", ",
                        from property in characteristicsType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        let propertyValue = property.GetValue(firstLemma.Characteristics)
                        where propertyValue != null
                        select $"{property.Name} = {property.PropertyType.RemoveNullabilityIfAny().Name}.{propertyValue}"));

                result.Append(" })");
            }

            return result.ToString();
        }

        private static readonly IDictionary<string, string> PunctuationMatcherNames = new Dictionary<string, string>
        { 
            { "-", "dash" },
            { ",", "comma" },
        };
    }
}