using System;
using System.Collections.Generic;
using System.Linq;
using ParseOzhegovWithSolarix.PredicateLogic;
using ParseOzhegovWithSolarix.Solarix;
using ParseOzhegovWithSolarix.SentenceStructureRecognizing;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Planning;

using Adjective = ParseOzhegovWithSolarix.Solarix.AdjectiveCharacteristics;
using Noun = ParseOzhegovWithSolarix.Solarix.NounCharacteristics;
using VerbalNoun = ParseOzhegovWithSolarix.Solarix.VerbalNounCharacteristics;
using Pronoun = ParseOzhegovWithSolarix.Solarix.PronounCharacteristics;
using Verb = ParseOzhegovWithSolarix.Solarix.VerbCharacteristics;

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
                var parsingResults = PrebuiltParsers
                    .Select(parserInfo =>
                    {
                        Sentence.MatchingResults.Clear();
                        var result = parserInfo.Value.Match(parsingVariant);
                        return new
                            {
                                ParserName = parserInfo.Key,
                                ParsingResult = AllInputElementsAreMatched(parsingVariant, Sentence.MatchingResults.Values) ? result : Optional.None<LogicPredicate>()
                        };
                    })
                    .Where(result => result.ParsingResult.HasValue)
                    .AsImmutable();

                switch (parsingResults.Count)
                {
                    case 0:
                        ParsingFailed?.Invoke(sentenceText, parsingVariant);
                        return null;

                    case 1:
                        return parsingResults.Single().ParsingResult.Value;

                    default:
                        throw new InvalidOperationException(
                            $"Program logic error: several parsers matched text '{sentenceText}'. " + 
                            $"Parser names are [{string.Join(", ", parsingResults.Select(item => item.ParserName))}]");
                }
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

        internal static readonly IDictionary<string, ISentenceElementMatcher<LogicFormula>> PrebuiltParsers = new Dictionary<string, ISentenceElementMatcher<LogicFormula>>
        {
            // P(x) 
            {
                "Сократ стар",
                from стар in Sentence.Root<Adjective>(new { Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from сократ in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                where стар.Detected.Gender == сократ.Detected.Gender
                select new LogicPredicate(стар.Lemma, new LogicVariable(сократ.Lemma))
            },
            {
                "Сократ - человек",
                from dash in (Sentence.Root(PartOfSpeech.Пунктуатор, "-" ) | Sentence.Root(PartOfSpeech.Местоим_Сущ, "это"))
                    from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma)
            },
            {
                "Сократ - это человек",
                from root in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from unused in root.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                    from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicVariable(objectName.Lemma), setName: setName.Lemma)
            },
            {
                "Сократ является человеком",
                from root in Sentence.Root(PartOfSpeech.Глагол, "является")
                    from setName in root.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                    from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicVariable(objectName.Lemma), setName: setName.Lemma)
            },
            {
                "Вода кипит",
                from root in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from subject in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new LogicPredicate(root.Lemma, new LogicVariable(subject.Lemma))
            },

            // ¬P(x)
            {
                "Сократ не стар",
                from predicate in Sentence.Root<Adjective>(new { Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from unused in predicate.NegationParticle(PartOfSpeech.Частица, "не")
                    from subject in predicate.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                where subject.Detected.Gender == predicate.Detected.Gender
                select new NegatedPredicate(new LogicPredicate(predicate.Lemma, new LogicVariable(subject.Lemma)))
            },
            {
                "Сократ не человек",
                from setName in Sentence.Root<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from unused in setName.NegationParticle(PartOfSpeech.Частица, "не")
                    from objectName in setName.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(objectName.Lemma), setName: setName.Lemma))
            },
            {
                "Сократ - не человек",
                from root in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from unused in root.NegationParticle(PartOfSpeech.Частица, "не")
                    from objectName in root.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from setName in root.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(objectName.Lemma), setName: setName.Lemma))
            },
            {
                "Сократ это не человек",
                from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from сократ in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma))
            },
            {
                "Сократ - это не человек",
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                    from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma))
            },
            {
                "Сократ не является человеком",
                from является in Sentence.Root(PartOfSpeech.Глагол, "является")
                    from не in является.NegationParticle(PartOfSpeech.Частица, "не")
                    from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                    from сократ in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человеком.Lemma))
            },
            {
                "Вода не кипит",
                from кипит in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from не in кипит.NegationParticle(PartOfSpeech.Частица, "не")
                    from вода in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(кипит.Lemma, new LogicVariable(вода.Lemma)))
            },
            {
                "неверно, что Сократ стар",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from стар in что.NextCollocationItem<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                                from сократ in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                where сократ.Detected.Gender == стар.Detected.Gender
                select new NegatedPredicate(new LogicPredicate(стар.Lemma, new LogicVariable(сократ.Lemma)))
            },
            {
                "неверно, что Сократ человек",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from человек in что.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                from сократ in человек.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma))
            },
            {
                "неверно, что Сократ - человек",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from dash in что.NextCollocationItem(PartOfSpeech.Пунктуатор, "-")
                                from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное})
                                from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma))
            },
            {
                "неверно, что Сократ это человек",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from это in что.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                                from сократ in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma))
            },
            {
                "неверно, что Сократ - это человек",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from dash in что.NextCollocationItem(PartOfSpeech.Пунктуатор, "-")
                                from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                                from сократ in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человек.Lemma))
            },
            {
                "неверно, что Сократ является человеком",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from является in что.NextCollocationItem(PartOfSpeech.Глагол, "является")
                                from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                                from сократ in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicVariable(сократ.Lemma), setName: человеком.Lemma))
            },
            {
                "неверно, что вода кипит",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from кипит in что.NextCollocationItem<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                                from вода in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(кипит.Lemma, new LogicVariable(вода.Lemma)))
            },

            // P(f(x)) 
            {
                "Отец Сократа стар",
                from стар in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from отец in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                where стар.Detected.Gender == отец.Detected.Gender
                select new LogicPredicate(стар.Lemma, new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)))
            },
            {
                "Отец Сократа - человек",
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from отец in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma)
            },
            {
                "Отец Сократа это человек",
                from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from отец in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma)
            },
            {
                "Отец Сократа - это человек",
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                        from отец in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                            from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma)
            },
            {
                "Отец Сократа является человеком",
                from является in Sentence.Root(PartOfSpeech.Глагол, "является")
                    from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                        from отец in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                            from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человеком.Lemma)
            },
            {
                "Содержимое кастрюли кипит",
                from кипит in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from содержимое in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from кастрюли in содержимое.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new LogicPredicate(кипит.Lemma, new LogicFunction(содержимое.Lemma, new LogicVariable(кастрюли.Lemma)))
            },

            // (∀ x) x ∈ S ⇒ P(x) 
            {
                "(каждый | любой) человек смертен",
                from смертен in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from человек in смертен.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from каждый in человек.Attribute<Adjective>(new { Lemma = "каждый" }, UglyHack_ThereIsGoingToBeAnotherVariantOfTheSameElement) |
                                       человек.Attribute<Adjective>(new { Lemma = "любой" })
                where смертен.Detected.Gender == человек.Detected.Gender && человек.Detected.Gender == каждый.Detected.Gender
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: человек.Lemma).Follows(new LogicPredicate(смертен.Lemma, variable)))
            },
            {
                "все люди смертны",
                from смертны in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Множественное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from люди in смертны.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                        from все in люди.Attribute(PartOfSpeech.Прилагательное, "все")
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: люди.Lemma).Follows(new LogicPredicate(смертны.Lemma, variable)))
            },
            {
                "все лебеди белые",
                from лебеди in Sentence.Root<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                    from все in лебеди.Attribute(PartOfSpeech.Прилагательное, "все")
                    from белые in лебеди.Attribute<Adjective>(new { Case = Case.Именительный, Number = Number.Множественное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new LogicPredicate(белые.Lemma, variable)))
            },

            // (∀ x) x ∈ S ⇒ x ∈ T
            {
                "Лебеди являются птицами",
                from являются in Sentence.Root(PartOfSpeech.Глагол, "являются")
                    from птицами in являются.Object<Noun>(new { Case = Case.Творительный, Number = Number.Множественное })
                    from лебеди in являются.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new SetContainsPredicate(setElement: variable, setName: птицами.Lemma)))
            },
            {
                "Лебеди это птицы",
                from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from лебеди in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                    from птицы in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new SetContainsPredicate(setElement: variable, setName: птицы.Lemma)))
            },
            {
                "Лебеди- это птицы",
                from лебеди in Sentence.Root<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                    from dash in лебеди.NextClause(PartOfSpeech.Пунктуатор, "-")
                        from птицы in dash.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                            from это in птицы.Subject(PartOfSpeech.Местоим_Сущ, "это")
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new SetContainsPredicate(setElement: variable, setName: птицы.Lemma)))
            },
            {
                "Каждый лебедь это птица",
                from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from лебедь in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from каждый in лебедь.Attribute<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                    from птица in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                where лебедь.Detected.Gender == каждый.Detected.Gender
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебедь.Lemma).Follows(new SetContainsPredicate(setElement: variable, setName: птица.Lemma)))
            },
            {
                "Каждый лебедь - это птица",
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                    from лебедь in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from каждый in лебедь.Attribute<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                    from птица in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                where лебедь.Detected.Gender == каждый.Detected.Gender
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Universal,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: лебедь.Lemma).Follows(new SetContainsPredicate(setElement: variable, setName: птица.Lemma)))
            },

            // (∃ x) x ∈ S ⇒ P(x) 
            {
                "существует(ют) живой(ые) композитор(ы)",
                from существует in Sentence.Root<Verb>(new { VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее, Lemma = "существовать" })
                    from композитор in существует.Subject<Noun>(new { Case = Case.Именительный })
                        from живой in композитор.Attribute<Adjective>(new { Case = Case.Именительный, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                where (существует.Detected.Number != Number.Единственное || композитор.Detected.Gender == живой.Detected.Gender) &&
                      существует.Detected.Number == композитор.Detected.Number && композитор.Detected.Number == живой.Detected.Number
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                            QuantifierType.Existential,
                            variable,
                            new SetContainsPredicate(setElement: variable, setName: композитор.Lemma) & (new LogicPredicate(живой.Lemma, variable)))
            },

            // P(x, y) 
            {
                "Вова любит Машу",
                from любит in Sentence.Root<Verb>(new { Case = Case.Винительный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from машу in любит.Object<Noun>(new { Case = Case.Винительный, Number = Number.Единственное })
                    from вова in любит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new LogicPredicate(любит.Lemma, new LogicVariable(вова.Lemma), new LogicVariable(машу.Lemma))
            },
            {
                "Вася выше Пети",
                from выше in Sentence.Root<Adjective>(new { AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Сравнительная })
                from пети in выше.Object<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                from вася in выше.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new LogicPredicate(выше.Content, new LogicVariable(вася.Lemma), new LogicVariable(пети.Lemma))
            },
            {
                "Вася выше, чем Петя",
                from выше in Sentence.Root<Adjective>(new { AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Сравнительная })
                    from comma in выше.Object(PartOfSpeech.Пунктуатор, ",")
                        from чем in comma.NextCollocationItem(PartOfSpeech.Союз, "чем")
                            from петя in чем.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from вася in выше.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new LogicPredicate(выше.Content, new LogicVariable(вася.Lemma), new LogicVariable(петя.Lemma))
            },

            // P(x) & Q(x) 
            {
                "Сократ стар и умен",
                from стар in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from и in стар.RightLogicItem(PartOfSpeech.Союз, "и")
                        from умен in и.NextCollocationItem<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from сократ in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                where сократ.Detected.Gender == стар.Detected.Gender && стар.Detected.Gender == умен.Detected.Gender
                let x = new LogicVariable(сократ.Lemma)
                select new LogicPredicate(стар.Lemma, x) & new LogicPredicate(умен.Lemma, x)
            },
            {
                "Роза цветет и пахнет",
                from цветет in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from и in цветет.RightLogicItem(PartOfSpeech.Союз, "и")
                        from пахнет in и.NextCollocationItem<Verb>(new { Case = Case.Творительный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from роза in цветет.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                let variable = new LogicVariable(роза.Lemma)
                select new LogicPredicate(цветет.Lemma, variable) & new LogicPredicate(пахнет.Lemma, variable)
            },
            {
                "Петя - человек и пароход",
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-") | Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from петя in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from и in человек.RightLogicItem(PartOfSpeech.Союз, "и")
                            from пароход in и.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                let variable = new LogicVariable(петя.Lemma)
                select new SetContainsPredicate(setElement: variable, setName: человек.Lemma) & new SetContainsPredicate(setElement: variable, setName: пароход.Lemma)
            },
            {
                "Петя - это человек и пароход",
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                    from петя in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from и in человек.RightLogicItem(PartOfSpeech.Союз, "и")
                            from пароход in и.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                let variable = new LogicVariable(петя.Lemma)
                select new SetContainsPredicate(setElement: variable, setName: человек.Lemma) & new SetContainsPredicate(setElement: variable, setName: пароход.Lemma)
            },

            // P(x) | Q(x) 
            {
                "Пациент жив или мертв",
                from жив in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from или in жив.RightLogicItem(PartOfSpeech.Союз, "или")
                        from мертв in или.NextCollocationItem<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from пациент in жив.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                where пациент.Detected.Gender == жив.Detected.Gender && жив.Detected.Gender == мертв.Detected.Gender
                let variable = new LogicVariable(пациент.Lemma)
                select new LogicPredicate(жив.Lemma, variable) | new LogicPredicate(мертв.Lemma, variable)
            },

            // P(x) ⇒ Q(x) 
            {
                "Если пациент жив, то он дышит",
                from жив in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from пациент in жив.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from если in жив.PrefixConjunction(PartOfSpeech.Союз, "Если")
                    from comma in жив.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from то in comma.NextCollocationItem(PartOfSpeech.Союз, "то")
                            from дышит in то.NextCollocationItem<Verb>(new { Case = Case.Творительный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                                from он in дышит.Subject<Pronoun>(new { Lemma = "я", Number = Number.Единственное, Person = Person.Третье })
                where пациент.Detected.Gender == жив.Detected.Gender && пациент.Detected.Gender == он.Detected.Gender
                let variable = new LogicVariable(пациент.Content)
                select new LogicPredicate(жив.Lemma, variable).Follows(new LogicPredicate(дышит.Lemma, variable))
            },
            {
                "Из того, что число натуральное, следует, что оно положительное",
                from следует in Sentence.Root(PartOfSpeech.Impersonal_Verb, "следует")
                    from из in следует.PreposAdjunct(PartOfSpeech.Предлог, "Из")
                        from того in из.Object(PartOfSpeech.Местоим_Сущ, "того")
                            from comma in того.Punctuation(PartOfSpeech.Пунктуатор, ",")
                            from число in того.SubordinateClause<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                from натуральное in число.SeparateAttribute<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                                from что in число.Subject(PartOfSpeech.Местоим_Сущ, "что")
                            from comma1 in того.Punctuation(PartOfSpeech.Пунктуатор, ",")
                    from comma2 in следует.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что1 in comma2.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from оно in что1.NextCollocationItem<Pronoun>(new { Number = Number.Единственное, Person = Person.Третье })
                                from положительное in оно.Attribute<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                where число.Detected.Gender == натуральное.Detected.Gender && натуральное.Detected.Gender == положительное.Detected.Gender
                let variable = new LogicVariable(число.Content)
                select new LogicPredicate(натуральное.Lemma, variable).Follows(new LogicPredicate(положительное.Lemma, variable))
            },

            // ¬P(f(x)) 
            {
                "Отец Сократа не стар",
                from стар in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from не in стар.NegationParticle(PartOfSpeech.Частица, "не")
                    from отец in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                where стар.Detected.Gender == отец.Detected.Gender
                select new NegatedPredicate(new LogicPredicate(стар.Lemma, new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma))))
            },
            {
                "Отец Сократа не человек",
                from отец in Sentence.Root<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from человек in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Множественное })
                        from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                        from сократа in человек.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma))
            },
            {
                "Отец Сократа - не человек",
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from не in dash.NegationParticle(PartOfSpeech.Частица, "не")
                    from отец in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma))
            },
            {
                "Отец Сократа это не человек",
                from это in Sentence.Root(PartOfSpeech.Местоим_Сущ, "это")
                    from отец in это.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in это.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma))
            },
            {
                "Отец Сократа - это не человек",
                from dash in Sentence.Root(PartOfSpeech.Пунктуатор, "-")
                    from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                    from отец in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from не in человек.NegationParticle(PartOfSpeech.Частица, "не")
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma))
            },
            {
                "Отец Сократа не является человеком",
                from является in Sentence.Root(PartOfSpeech.Глагол, "является")
                    from не in является.NegationParticle(PartOfSpeech.Частица, "не")
                    from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                    from отец in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человеком.Lemma))
            },
            {
                "Содержимое кастрюли не кипит",
                from кипит in Sentence.Root<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from не in кипит.NegationParticle(PartOfSpeech.Частица, "не")
                    from содержимое in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from кастрюли in содержимое.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(кипит.Lemma, new LogicFunction(содержимое.Lemma, new LogicVariable(кастрюли.Lemma))))
            },
            {
                "неверно, что отец Сократа стар",
                from неверно in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from стар in что.NextCollocationItem<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                                from отец in стар.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(стар.Lemma, new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma))))
            },
            {
                "неверно, что отец Сократа -  человек",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from dash in что.NextCollocationItem(PartOfSpeech.Пунктуатор, "-", UglyHack_ThereIsGoingToBeAnotherVariantOfTheSameElement) | что.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                                from отец in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                                from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma))
            },
            {
                "неверно, что отец Сократа  - это человек",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from dash in что.NextCollocationItem(PartOfSpeech.Пунктуатор, "-")
                                from это in dash.NextCollocationItem(PartOfSpeech.Местоим_Сущ, "это")
                                from отец in dash.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                                from человек in dash.Rhema<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человек.Lemma))
            },
            {
                "неверно, что отец Сократа является человеком",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from является in что.NextCollocationItem<Verb>(new { Case = Case.Дательный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                                from человеком in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                                from отец in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from сократа in отец.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new NegatedPredicate(new SetContainsPredicate(setElement: new LogicFunction(отец.Lemma, new LogicVariable(сократа.Lemma)), setName: человеком.Lemma))
            },
            {
                "неверно, что содержимое кастрюли кипит",
                from неверно in Sentence.Root(PartOfSpeech.Прилагательное, "неверно")
                    from comma in неверно.NextClause(PartOfSpeech.Пунктуатор, ",")
                        from что in comma.NextCollocationItem(PartOfSpeech.Союз, "что")
                            from кипит in что.NextCollocationItem<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                                from содержимое in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from кастрюли in содержимое.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(кипит.Lemma, new LogicFunction(содержимое.Lemma, new LogicVariable(кастрюли.Lemma))))
            },

            // (∀ x) x ∈ S ⇒  P(f(x)) 
            {
                "Цвет любого лебедя белый",
                from цвет in Sentence.Root<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from лебедя in цвет.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                        from любого in лебедя.Attribute<Adjective>(new { Lemma = "любой", Case = Case.Родительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                    from белый in цвет.Attribute<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                where белый.Detected.Gender == цвет.Detected.Gender && белый.Detected.Gender == любого.Detected.Gender
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Universal,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: лебедя.Lemma).Follows(new LogicPredicate(белый.Lemma, new LogicFunction(цвет.Lemma, variable))))
            },
            {
                "Все лебеди белого цвета",
                from лебеди in Sentence.Root<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                    from все in лебеди.Attribute<Adjective>(new { Case = Case.Именительный, Number = Number.Множественное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                    from цвета in лебеди.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                        from белого in цвета.Attribute<Adjective>(new { Case = Case.Родительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                where цвета.Detected.Gender == белого.Detected.Gender
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Universal,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma).Follows(new LogicPredicate(белого.Lemma, new LogicFunction(цвета.Lemma, variable))))
            },

            // (∃ x) x ∈ S ⇒  P(f(x)) 
			{
                "существует(ют) лебедь(и) белого цвета",
                from существуют in Sentence.Root<Verb>(new { VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее, Lemma = "существовать" })
                    from лебеди in существуют.Subject<Noun>(new { Case = Case.Именительный })
                        from цвета in лебеди.RightGenitiveObject<Noun>(new { Case = Case.Родительный })
                            from белого in цвета.Attribute<Adjective>(new { Case = Case.Родительный, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                where цвета.Detected.Gender == белого.Detected.Gender && существуют.Detected.Number == лебеди.Detected.Number
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Existential,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: лебеди.Lemma) & new LogicPredicate(белого.Lemma, new LogicFunction(цвета.Lemma, variable)))
            },
            {
                "существует(ют) кастрюля(и), содержимое которой(ых) кипит",
                from существует in Sentence.Root<Verb>(new { VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее, Lemma = "существовать" })
                    from кастрюля in существует.Subject<Noun>(new { Case = Case.Именительный })
                        from comma in кастрюля.Attribute(PartOfSpeech.Пунктуатор, ",")
                            from кипит in comma.SubordinateClause<Verb>(new { Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                                from содержимое in кипит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                                    from которой in содержимое.RightGenitiveObject<Adjective>(new { Case = Case.Родительный, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                where существует.Detected.Number == кастрюля.Detected.Number && кастрюля.Detected.Number == которой.Detected.Number &&
                      (кастрюля.Detected.Number == Number.Множественное || кастрюля.Detected.Gender == которой.Detected.Gender)
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Existential,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: кастрюля.Lemma) & new LogicPredicate(кипит.Lemma, new LogicFunction(содержимое.Lemma, variable)))
            },

            // (∀ x) x ∈ S ⇒  ¬P(x)
            {
                "все мужчины не беременны",
                from беременны in Sentence.Root<Adjective>(new { Case = Case.Именительный, Number = Number.Множественное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from не in беременны.NegationParticle(PartOfSpeech.Частица, "не")
                    from мужчины in беременны.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Множественное })
                        from все in мужчины.Attribute(PartOfSpeech.Прилагательное, "все")
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Universal,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: мужчины.Lemma).Follows(
                        new NegatedPredicate(new LogicPredicate(беременны.Lemma, variable))))
            },
            {
                "не существует беременных мужчин",
                from существует in Sentence.Root(PartOfSpeech.Глагол, "существует")
                    from не in существует.NegationParticle(PartOfSpeech.Частица, "не")
                    from мужчин in существует.Object<Noun>(new { Case = Case.Родительный, Number = Number.Множественное })
                        from беременных in мужчин.Attribute<Adjective>(new { Case = Case.Родительный, Number = Number.Множественное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Universal,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: мужчин.Lemma).Follows(
                        new NegatedPredicate(new LogicPredicate(беременных.Lemma, variable))))
            },
            {
                "ни один мужчина не стар",
                from беременен in Sentence.Root<Adjective>(new { Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                    from не in беременен.NegationParticle(PartOfSpeech.Частица, "не")
                    from мужчина in беременен.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from один in мужчина.Attribute<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                        from ни in мужчина.PrefixParticle(PartOfSpeech.Частица, "ни")
                where беременен.Detected.Gender == мужчина.Detected.Gender && мужчина.Detected.Gender == один.Detected.Gender
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Universal,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: мужчина.Lemma).Follows(
                        new NegatedPredicate(new LogicPredicate(беременен.Lemma, variable))))
            },

            // (∃ x) x ∈ S ⇒  ¬P(x)
            {
                "существует(ют) не беременная(ые) женщина(ы)",
                from существуют in Sentence.Root<Verb>(new { VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее, Lemma = "существовать" })
                    from женщина in существуют.Subject<Noun>(new { Case = Case.Именительный })
                        from беременная in женщина.Attribute<Adjective>(new { Case = Case.Именительный, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                            from не in беременная.NegationParticle(PartOfSpeech.Частица, "не")
                where женщина.Detected.Number == беременная.Detected.Number && существуют.Detected.Number == женщина.Detected.Number &&
                     (женщина.Detected.Number == Number.Множественное || женщина.Detected.Gender == беременная.Detected.Gender)
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Existential,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: женщина.Lemma) & new NegatedPredicate(new LogicPredicate(беременная.Lemma, variable)))
            },
            {
                "существует женщина, которая не беременна",
                from существует in Sentence.Root(PartOfSpeech.Глагол, "существует")
                    from женщина in существует.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from comma in женщина.Attribute(PartOfSpeech.Пунктуатор, ",")
                            from беременна in comma.SubordinateClause<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Краткое, ComparisonForm = ComparisonForm.Атрибут })
                                from не in беременна.NegationParticle(PartOfSpeech.Частица, "не")
                                from которая in беременна.Subject<Adjective>(new { Case = Case.Именительный, Number = Number.Единственное, AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Атрибут })
                where женщина.Detected.Gender == которая.Detected.Gender && которая.Detected.Gender == беременна.Detected.Gender
                let variable = new LogicVariable("x")
                select new LogicQuantifier(
                    QuantifierType.Existential,
                    variable,
                    new SetContainsPredicate(setElement: variable, setName: женщина.Lemma) & new NegatedPredicate(new LogicPredicate(беременна.Lemma, variable)))
            },

            // ¬P(x, y) 
			{
                "Вова не любит Машу",
                from любит in Sentence.Root<Verb>(new { Case = Case.Винительный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from не in любит.NegationParticle(PartOfSpeech.Частица, "не")
                    from машу in любит.Object<Noun>(new { Case = Case.Винительный, Number = Number.Единственное })
                    from вова in любит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(любит.Lemma, new LogicVariable(вова.Lemma), new LogicVariable(машу.Lemma)))
            },
            {
                "Вася не выше Пети",
                from выше in Sentence.Root<Adjective>(new { AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Сравнительная })
                    from пети in выше.Object<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from не in выше.NegationParticle(PartOfSpeech.Частица, "не")
                    from вася in выше.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(выше.Content, new LogicVariable(вася.Lemma), new LogicVariable(пети.Lemma)))
            },
            {
                "Вася не выше, чем Петя",
                from выше in Sentence.Root<Adjective>(new { AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Сравнительная })
                    from comma in выше.Object(PartOfSpeech.Пунктуатор, ",")
                        from чем in comma.NextCollocationItem(PartOfSpeech.Союз, "чем")
                            from петя in чем.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from не in выше.NegationParticle(PartOfSpeech.Частица, "не")
                    from вася in выше.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                select new NegatedPredicate(new LogicPredicate(выше.Content, new LogicVariable(вася.Lemma), new LogicVariable(петя.Lemma)))
            },

            // P(f(x), y) 
            {
                "Мамой Пети является Маша",
                from является in Sentence.Root(PartOfSpeech.Глагол, "является")
                    from маша in является.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from мамой in является.Object<Noun>(new { Case = Case.Творительный, Number = Number.Единственное })
                        from пети in мамой.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new EqualityPredicate(new LogicFunction(мамой.Lemma, new LogicVariable(пети.Lemma)), new LogicVariable(маша.Lemma))
            },
            {
                "Папа Вовы любит Машу",
                from любит in Sentence.Root<Verb>(new { Case = Case.Винительный, Number = Number.Единственное, VerbForm = VerbForm.Изъявительное, Person = Person.Третье, VerbAspect = VerbAspect.Несовершенный, Tense = Tense.Настоящее })
                    from машу in любит.Object<Noun>(new { Case = Case.Винительный, Number = Number.Единственное })
                    from папа in любит.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from вовы in папа.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new LogicPredicate(любит.Lemma, new LogicFunction(папа.Lemma, new LogicVariable(вовы.Lemma)), new LogicVariable(машу.Lemma))
            },
            {
                "Папа Васи выше Пети",
                from выше in Sentence.Root<Adjective>(new { AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Сравнительная })
                    from пети in выше.Object<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                    from папа in выше.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from васи in папа.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new LogicPredicate(выше.Content, new LogicFunction(папа.Lemma, new LogicVariable(васи.Lemma)), new LogicVariable(пети.Lemma))
            },
            {
                "Папа Васи выше, чем Петя",
                from выше in Sentence.Root<Adjective>(new { AdjectiveForm = AdjectiveForm.Полное, ComparisonForm = ComparisonForm.Сравнительная })
                    from comma in выше.Object(PartOfSpeech.Пунктуатор, ",")
                        from чем in comma.NextCollocationItem(PartOfSpeech.Союз, "чем")
                            from петя in чем.NextCollocationItem<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                    from папа in выше.Subject<Noun>(new { Case = Case.Именительный, Number = Number.Единственное })
                        from васи in папа.RightGenitiveObject<Noun>(new { Case = Case.Родительный, Number = Number.Единственное })
                select new LogicPredicate(выше.Content, new LogicFunction(папа.Lemma, new LogicVariable(васи.Lemma)), new LogicVariable(петя.Lemma))
            }
        };

        private const bool UglyHack_ThereIsGoingToBeAnotherVariantOfTheSameElement = true;
    }
}