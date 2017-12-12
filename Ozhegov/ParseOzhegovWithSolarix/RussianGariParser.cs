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
        public RussianGariParser(IRussianGrammarParser russianGrammarParser)
        {
            _russianGrammarParser = russianGrammarParser;
        }

        public LogicFormula ParseSentence(string sentenceText)
        {
            var sentenceStructure = _russianGrammarParser.Parse(sentenceText);
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

                ParsingFailed?.Invoke(sentenceText, parsingVariant);
            }

            return null;
        }

        public void Dispose()
        {
            _russianGrammarParser.Dispose();
        }

        public event Action<string, SentenceElement> ParsingFailed;

        private static bool AllInputElementsAreMatched(SentenceElement sentenceElement, ICollection<ElementMatchingResult> elementMatchingResults)
        {
            return
                sentenceElement.LemmaVersions.Any(
                    inputLemmaVersion => elementMatchingResults.Any(
                        result => ReferenceEquals(result.LemmaVersion, inputLemmaVersion))) &&
                sentenceElement.Children.All(
                    childElement => AllInputElementsAreMatched(childElement, elementMatchingResults));
        }

        private readonly IRussianGrammarParser _russianGrammarParser;

        private static readonly ISentenceElementMatcher<LogicFormula>[] PrebuiltParsers = new ISentenceElementMatcher<LogicFormula>[]
            {
            // P(x) 
                // Сократ стар
                from стар in Sentence.Root<Adjective>(new { Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from сократ in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                where стар.Detected.Gender == сократ.Detected.Gender
                select new LogicPredicate(стар.Lemma, new LogicVariable(сократ.Lemma)),

                // Сократ - человек
                from dash in (Sentence.Root(PartOfSpeech.Пунктуатор, "-" ) | Sentence.Root(PartOfSpeech.Местоим_Сущ, "это"))
                    from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma),

                // Сократ - это человек
                from root in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from unused in root.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                    from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicVariable(objectName.Lemma), setName: setName.Lemma),

                // Сократ является человеком
                from root in Sentence.Root(PartOfSpeech.Глагол, "является")
                    from setName in root.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                    from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicVariable(objectName.Lemma), setName: setName.Lemma),

                // Вода кипит
                from root in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from subject in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new LogicPredicate(root.Lemma, new LogicVariable(subject.Lemma)),

            // ¬P(x)
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
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(objectName.Lemma), setName: setName.Lemma)),

                // Сократ - не человек
                from root in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from unused in root.NegationParticle(PartOfSpeech.Частица, "не")
                    from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(objectName.Lemma), setName: setName.Lemma)),

                // Сократ это не человек
                from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from сократ in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma)),

                // Сократ - это не человек
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                    from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma)),

                // Сократ не является человеком
                from является in Sentence.Root(PartOfSpeech.Глагол, "является")
                    from не in является.NegationParticle(PartOfSpeech.Частица, "не")
                    from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                    from сократ in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человеком.Lemma)),

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
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma)),

                // неверно, что Сократ - человек
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from dash in что.NextCollocationItem(PartOfSpeech.Пунктуатор, "-")
                                from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное})
                                from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma)),

                // неверно, что Сократ это человек
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from это in что.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                                from сократ in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma)),

                // неверно, что Сократ - это человек
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from dash in что.NextCollocationItem(PartOfSpeech.Пунктуатор, "-")
                                from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                                from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma)),

                // неверно, что Сократ является человеком
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from является in что.NextCollocationItem(PartOfSpeech.Глагол, "является")
                                from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                                from сократ in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человеком.Lemma)),

                // неверно, что вода кипит
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from кипит in что.NextCollocationItem<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                                from вода in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(кипит.Lemma, new LogicVariable(вода.Lemma))),

            // P(f(x)) 
                // Отец Сократа стар
                from стар in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from отец in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                where стар.Detected.Gender == отец.Detected.Gender
                select new LogicPredicate(стар.Lemma, new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma))),

                // Отец Сократа - человек
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from отец in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma),

                // Отец Сократа это человек
                from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from отец in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma),

                // Отец Сократа - это человек
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                        from отец in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                            from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma),

                // Отец Сократа является человеком
                from является in Sentence.Root(PartOfSpeech.Глагол, "является")
                    from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                        from отец in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                            from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человеком.Lemma),

                // Содержимое кастрюли кипит
                from кипит in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from содержимое in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from кастрюли in содержимое.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new LogicPredicate(кипит.Lemma, new LogicFunction(содержимое.Lemma, new LogicVariable(кастрюли.Lemma))),

            // (∀ x) x ∈ S ⇒ P(x) 
                // (каждый | любой) человек смертен
                from смертен in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from человек in смертен.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from каждый in человек.Attribute<Adjective>(new { Lemma = "каждый" }, UglyHack_ThereIsGoingToBeAnotherVariantOfTheSameElement) |
                                       человек.Attribute<Adjective>(new { Lemma = "любой" })
                where смертен.Detected.Gender == человек.Detected.Gender && человек.Detected.Gender == каждый.Detected.Gender
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: человек.Lemma).Follows(new LogicPredicate(смертен.Lemma, variable))),

                // все люди смертны
                from смертны in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Множественное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from люди in смертны.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                        from все in люди.Attribute(PartOfSpeech.Прилагательное, "все")
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: люди.Lemma).Follows(new LogicPredicate(смертны.Lemma, variable))),

                // все лебеди белые
                from белые in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Множественное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                    from лебеди in белые.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                        from все in лебеди.Attribute(PartOfSpeech.Прилагательное, "все")
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new LogicPredicate(белые.Lemma, variable))),

            // (∀ x) x ∈ S ⇒ x ∈ T
                // Лебеди являются птицами
                from являются in Sentence.Root(PartOfSpeech.Глагол, "являются")
                    from птицами in являются.Object<Noun>(new { Case = Case.Творительный, Number = Number.Множественное })
                    from лебеди in являются.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new SetContainsPredicate(setElement: variable, setName: птицами.Lemma))),

                // Лебеди это птицы
                from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from лебеди in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                    from птицы in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new SetContainsPredicate(setElement: variable, setName: птицы.Lemma))),

                // Лебеди- это птицы
                from это in Sentence.Root(PartOfSpeech.Прилагательное, "это")
                    from dash in это.PrefixParticle(PartOfSpeech.Пунктуатор, "-")
                        from лебеди in dash.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                    from птицы in это.Object<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new SetContainsPredicate(setElement: variable, setName: птицы.Lemma))),

            // (∃ x) x ∈ S ⇒ P(x) 
                // существует(ют) живой(ые) композитор(ы)
                from существует in Sentence.Root<Verb>(new { VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее, Lemma = "существовать" })
                    from композитор in существует.Subject<Noun>(new { Case = Case.Именительный })
                        from живой in композитор.Attribute<Adjective>(new { Case = Case.Именительный, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                where (существует.Detected.Number != Number.Единственное || композитор.Detected.Gender == живой.Detected.Gender) &&
                      существует.Detected.Number == композитор.Detected.Number && композитор.Detected.Number == живой.Detected.Number
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Existential,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: композитор.Lemma) & (new LogicPredicate(живой.Lemma, variable))),

           // P(x, y) 
                // Вова любит Машу
                from любит in Sentence.Root<Verb>(new { Case = Case.Винительный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from машу in любит.Object<Noun>(new { Case = Case.Винительный, Number = Number.Единственное })
                    from вова in любит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new LogicPredicate(любит.Lemma, new LogicVariable(вова.Lemma), new LogicVariable(машу.Lemma)),

                // Вася выше Пети
                from выше in Sentence.Root<Adjective>(new { AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Сравнительная })
                from пети in выше.Object<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                from вася in выше.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new LogicPredicate(выше.Content, new LogicVariable(вася.Lemma), new LogicVariable(пети.Lemma)),

                // Вася выше, чем Петя
                from выше in Sentence.Root<Adjective>(new { AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Сравнительная })
                    from comma in выше.Object(PartOfSpeech.Пунктуатор, ",")
                        from чем in comma.NextCollocationItem(PartOfSpeech.Союз, "чем")
                            from петя in чем.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from вася in выше.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new LogicPredicate(выше.Content, new LogicVariable(вася.Lemma), new LogicVariable(петя.Lemma))
                };

        private const bool UglyHack_ThereIsGoingToBeAnotherVariantOfTheSameElement = true;
    }
}