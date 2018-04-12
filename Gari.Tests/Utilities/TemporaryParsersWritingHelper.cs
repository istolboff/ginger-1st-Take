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
            const string initialLinePrefix = "\t\t\t";
            Trace.WriteLine(initialLinePrefix + "{");
            Trace.WriteLine(initialLinePrefix + $"\t\"{sentenceText}\",");
            DumpParsingExpressionSkeletonCore(sentenceElement, "Sentence", new HashSet<string>(), initialLinePrefix + "\t");
            Trace.WriteLine(initialLinePrefix + "\tselect new ??????");
            Trace.WriteLine(initialLinePrefix + "}");
        }

        public static void DumpParsingExpressionSkeletons(IEnumerable<string> sentences, IRussianGrammarParser russianGrammarParser)
        {
            foreach (var sentence in sentences)
            {
                DumpParsingExpressionSkeleton(sentence, russianGrammarParser.Parse(sentence).First());
                Trace.WriteLine(string.Empty);
            }
        }

        private static void DumpParsingExpressionSkeletonCore(
            SentenceElement sentenceElement, 
            string parentMatcherName,
            ICollection<string> usedVariableNames,
            string linePrefix)
        {
            var matcherName = GetMatcherName(sentenceElement.Content, usedVariableNames);
            Trace.WriteLine($"{linePrefix}from {matcherName} in {parentMatcherName}.{CreateSelectingMethod(sentenceElement)}");
            usedVariableNames.Add(matcherName);
            foreach (var child in sentenceElement.Children)
            {
                DumpParsingExpressionSkeletonCore(child, matcherName, usedVariableNames, linePrefix + "\t");
            }
        }

        private static string GetMatcherName(string content, ICollection<string> usedVariableNames)
        {
            var result = (PunctuationMatcherNames.TryGetValue(content, out var specialName) ? specialName : content).ToLower();
            return Enumerable.Range(0, 100).Select(i => result + ((int?)i).If(v => v > 0)).First(name => !usedVariableNames.Contains(name));
        }

        private static string CreateSelectingMethod(SentenceElement sentenceElement)
        {
            var result = new StringBuilder();
            result.Append(sentenceElement.LeafLinkType == null ? "Root" : LinkTypeName[sentenceElement.LeafLinkType.Value]);

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
                        where property.PropertyType.RemoveNullabilityIfAny() != typeof(Form) && propertyValue != null
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

        private static readonly VerboseIndexer<LinkType, string> LinkTypeName = new Dictionary<LinkType, string>
                {
                    { LinkType.SUBJECT_link, "Subject" },
                    { LinkType.OBJECT_link, "Object" },
                    { LinkType.RHEMA_link, "Rhema" },
                    { LinkType.NEXT_COLLOCATION_ITEM_link, "NextCollocationItem" },
                    { LinkType.NEGATION_PARTICLE_link, "NegationParticle" },
                    { LinkType.NEXT_CLAUSE_link, "NextClause" },
                    { LinkType.RIGHT_GENITIVE_OBJECT_link, "RightGenitiveObject" },
                    { LinkType.ATTRIBUTE_link,  "Attribute" },
                    { LinkType.RIGHT_NAME_link, "RightName" },
                    { LinkType.SEPARATE_ATTR_link, "SeparateAttribute" },
                    { LinkType.PREFIX_PARTICLE_link, "PrefixParticle" },
                    { LinkType.RIGHT_LOGIC_ITEM_link, "RightLogicItem" },
                    { LinkType.DETAILS_link, "Details" },
                    { LinkType.PUNCTUATION_link, "Punctuation" },
                    { LinkType.PREFIX_CONJUNCTION_link, "PrefixConjunction" },
                    { LinkType.SUBORDINATE_CLAUSE_link, "SubordinateClause" },
                    { LinkType.PREPOS_ADJUNCT_link, "PreposAdjunct" },
                    { LinkType.NEXT_ADJECTIVE_link, "NextAdjective" },
                    { LinkType.BEG_INTRO_link, "BegIntro" },
                    { LinkType.INFINITIVE_link, "Infinitive" }
                }
                .WithVerboseIndexing("LinkType");
    }
}