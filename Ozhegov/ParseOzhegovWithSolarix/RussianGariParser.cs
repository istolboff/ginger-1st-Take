using System;
using System.Collections.Generic;
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
                        from subject in predicate.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    where predicate.Detected.Gender == subject.Detected.Gender
                    select new LogicPredicate(predicate.Lemma, new LogicVariable(subject.Lemma)),

                    // Сократ (-|это) человек
                    from root in (Sentence.Root(PartOfSpeech.Пунктуатор, "-" ) | Sentence.Root(PartOfSpeech.Местоим_Сущ, "это"))
                        from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new SetContainsPredicate(objectName: objectName.Lemma, setName: setName.Lemma),

                    // Сократ - это человек
                    from root in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                        from unused in root.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                        from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
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
                        from subject in predicate.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    where subject.Detected.Gender == predicate.Detected.Gender
                    select new NegatedPredicate(new LogicPredicate(predicate.Lemma, new LogicVariable(subject.Lemma))),

                    // Сократ не человек
                    from setName in Sentence.Root<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from unused in setName.NegationParticle(PartOfSpeech.Частица, "не")
                        from objectName in setName.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new SetContainsPredicate(objectName: objectName.Lemma, setName: setName.Lemma)),

                    // Сократ - не человек
                    from root in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                        from unused in root.NegationParticle(PartOfSpeech.Частица, "не")
                        from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new SetContainsPredicate(objectName: objectName.Lemma, setName: setName.Lemma)),

                    // Сократ это не человек
                    from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                        from сократ in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                            from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                    select new NegatedPredicate(new SetContainsPredicate(objectName: сократ.Lemma, setName: человек.Lemma)),

                    // Сократ - это не человек
                    from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                        from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                        from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                            from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                    select new NegatedPredicate(new SetContainsPredicate(objectName: сократ.Lemma, setName: человек.Lemma)),

                    // Сократ не является человеком
                    from является in Sentence.Root<Verb>(new { Case = Case.Дательный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                        from не in является.NegationParticle(PartOfSpeech.Частица, "не")
                        from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                        from сократ in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new SetContainsPredicate(objectName: сократ.Lemma, setName: человеком.Lemma)),

                    // Вода не кипит
                    from кипит in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                        from не in кипит.NegationParticle(PartOfSpeech.Частица, "не")
                        from вода in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new LogicPredicate(кипит.Lemma, new LogicVariable(вода.Lemma))),

                    // неверно, что Сократ стар
                    from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                        from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                            from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                                from стар in что.NextCollocationItem<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                                    from сократ in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    where сократ.Detected.Gender == стар.Detected.Gender
                    select new NegatedPredicate(new LogicPredicate(стар.Lemma, new LogicVariable(сократ.Lemma))),

                    // неверно, что Сократ человек
                    from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                        from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                            from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                                from человек in что.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from сократ in человек.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new SetContainsPredicate(objectName: сократ.Lemma, setName: человек.Lemma)),

                    // неверно, что Сократ - человек
                    from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                        from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                            from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                                from dash in что.NextCollocationItem(PartOfSpeech.Пунктуатор, "-")
                                    from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное})
                                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new SetContainsPredicate(objectName: сократ.Lemma, setName: человек.Lemma)),

                    // неверно, что Сократ это человек
                    from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                        from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                            from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                                from это in что.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                                    from сократ in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new SetContainsPredicate(objectName: сократ.Lemma, setName: человек.Lemma)),

                    // неверно, что Сократ - это человек
                    from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                        from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                            from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                                from dash in что.NextCollocationItem(PartOfSpeech.Пунктуатор, "-")
                                    from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                                    from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new SetContainsPredicate(objectName: сократ.Lemma, setName: человек.Lemma)),

                    // неверно, что Сократ является человеком
                    from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                        from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                            from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                                from является in что.NextCollocationItem<Verb>(new { Case = Case.Дательный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                                    from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                                    from сократ in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new SetContainsPredicate(objectName: сократ.Lemma, setName: человеком.Lemma)),

                    // неверно, что вода кипит
                    from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                        from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                            from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                                from кипит in что.NextCollocationItem<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                                    from вода in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    select new NegatedPredicate(new LogicPredicate(кипит.Lemma, new LogicVariable(вода.Lemma)))
                };

            var sentenceStructure = _russianGrammarEngine.Parse(sentenceText);
            foreach (var parsingVariant in sentenceStructure)
            {
                var parsingResult = PrebuiltParsers
                    .Select(parser =>
                    {
                        Sentence.MatchingResults.Clear();
                        var result = parser.Match(parsingVariant);
                        return AllInputElementsAreMatched(parsingVariant, Sentence.MatchingResults.Values) ? result : Optional<LogicPredicate>.None;
                    })
                    .FirstOrDefault(result => result.HasValue) ?? Optional<LogicPredicate>.None;

                if (parsingResult.HasValue)
                {
                    return parsingResult.Value;
                }

                TemporaryParsersWritingHelper.DumpParsingExpressionSkeleton(sentenceText, parsingVariant);
            }

            return null;
        }

        public void Dispose()
        {
            _russianGrammarEngine.Dispose();
        }

        private static bool AllInputElementsAreMatched(SentenceElement sentenceElement, ICollection<ElementMatchingResult> elementMatchingResults)
        {
            return
                sentenceElement.LemmaVersions.Any(
                    inputLemmaVersion => elementMatchingResults.Any(
                        result => ReferenceEquals(result.LemmaVersion, inputLemmaVersion))) &&
                sentenceElement.Children.All(
                    childElement => AllInputElementsAreMatched(childElement, elementMatchingResults));
        }

        private readonly SolarixRussianGrammarEngine _russianGrammarEngine = new SolarixRussianGrammarEngine();
    }
}