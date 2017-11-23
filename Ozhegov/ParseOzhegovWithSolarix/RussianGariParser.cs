using System;
using System.Linq;
using ParseOzhegovWithSolarix.PredicateLogic;
using ParseOzhegovWithSolarix.Solarix;
using ParseOzhegovWithSolarix.SentenceStructureRecognizing;

using Noun = ParseOzhegovWithSolarix.Solarix.NounCharacteristics;
using Adjective = ParseOzhegovWithSolarix.Solarix.AdjectiveCharacteristics;
using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix
{
    public sealed class RussianGariParser : IDisposable
    {
        public RussianGariParser()
        {
            _russianGrammarEngine.Initialize();
        }

        public object ParseSentence(string sentenceText)
        {
            var sentenceStructure = _russianGrammarEngine.Parse(sentenceText);
            foreach (var parsingVariant in sentenceStructure)
            {
                var parsingResult = PrebuiltParsers
                    .Select(parser =>
                    {
                        Sentence.MatchingResults.Clear();
                        return parser(parsingVariant);
                    })
                    .FirstOrDefault(result => result.HasValue);

                if (parsingResult.HasValue)
                {
                    return parsingResult.Value;
                }

                return null;
            }

            return null;
        }

        public void Dispose()
        {
            _russianGrammarEngine.Dispose();
        }

        private readonly SolarixRussianGrammarEngine _russianGrammarEngine = new SolarixRussianGrammarEngine();

        private static readonly Func<SentenceElement, Optional<LogicPredicate>>[] PrebuiltParsers = new[] 
            {
                // Сократ стар
                from predicate in Sentence.Root<Adjective>(new { Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from subject in predicate.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное, Gender = predicate.Detected.Gender })
                select new LogicPredicate(predicate.Lemma, new LogicVariable(subject.Lemma)),

                //from root in (Sentence.Root(PartOfSpeech.Пунктуатор, "-" ) | Sentence.Root(PartOfSpeech.Местоим_Сущ, "это"))
                //    from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                //    from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное, Gender = objectName.Gender })
                //select new SetContainsPredicate(objectName: objectName.Lemma, setName: setName.Lemma),

                //from root in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                //    from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                //    from unused in root.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                //    from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное, Gender = objectName.Gender })
                //select new SetContainsPredicate(objectName: objectName.Lemma, setName: setName.Lemma)
            };
    }
}
