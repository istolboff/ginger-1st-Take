using System;
using System.Linq;
using ParseOzhegovWithSolarix.PredicateLogic;
using ParseOzhegovWithSolarix.Solarix;
using ParseOzhegovWithSolarix.SentenceStructureRecognizing;
using ParseOzhegovWithSolarix.Miscellaneous;

using Noun = ParseOzhegovWithSolarix.Solarix.NounCharacteristics;
using Verb = ParseOzhegovWithSolarix.Solarix.VerbCharacteristics;
using Adjective = ParseOzhegovWithSolarix.Solarix.AdjectiveCharacteristics;

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
            var PrebuiltParsers = new ISentenceElementMatcher<LogicPredicate>[]
                {
                    // Сократ стар
                    from predicate in Sentence.Root<Adjective>(new { Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                        from subject in predicate.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное, Gender = predicate.Detected.Gender })
                    select new LogicPredicate(predicate.Lemma, new LogicVariable(subject.Lemma)),

                    // Сократ (-|это) человек
                    from root in (Sentence.Root(PartOfSpeech.Пунктуатор, "-" ) | Sentence.Root(PartOfSpeech.Местоим_Сущ, "это"))
                        from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное, Gender = objectName.Detected.Gender })
                    select new SetContainsPredicate(objectName: objectName.Lemma, setName: setName.Lemma),

                    // Сократ - это человек
                    from root in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                        from unused in root.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                        from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное, Gender = objectName.Detected.Gender })
                    select new SetContainsPredicate(objectName: objectName.Lemma, setName: setName.Lemma),

                    // Сократ является человеком
                    from root in Sentence.Root<Verb>(new { Case = Case.Дательный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                        from setName in root.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                        from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new SetContainsPredicate(objectName: objectName.Lemma, setName: setName.Lemma),

                    // Вода кипит
                    from root in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                        from subject in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new LogicPredicate(root.Lemma, new LogicVariable(subject.Lemma)),

                    // Сократ не стар
                    from predicate in Sentence.Root<Adjective>(new { Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                        from unused in predicate.NegationParticle(PartOfSpeech.Частица, "не")
                        from subject in predicate.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное, Gender = predicate.Detected.Gender })
                    select new NegatedPredicate(new LogicPredicate(predicate.Lemma, new LogicVariable(subject.Lemma)))
                };

            var sentenceStructure = _russianGrammarEngine.Parse(sentenceText);
            foreach (var parsingVariant in sentenceStructure)
            {
                var parsingResult = PrebuiltParsers
                    .Select(parser =>
                    {
                        Sentence.MatchingResults.Clear();
                        return parser.Match(parsingVariant);
                    })
                    .FirstOrDefault(result => result.HasValue) ?? Optional<LogicPredicate>.None;

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
    }
}