﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParseOzhegovWithSolarix;
using ParseOzhegovWithSolarix.Solarix;
using Gari.Tests.Utilities;
using System.Linq;

namespace Gari.Tests
{
    [TestClass]
    public sealed class BasicNaturalLanguageSentenceParsingTests
    {
        [TestMethod]
        public void ElementaryNaturalLanguageSentencesShouldBeCorrectlyParsedToLogicalExpressions()
        {
            VerifyCorrectParsing(ParsingSamples);
        }

        [TestMethod]
        public void CheckingGenderVariateionsInBasicStatements()
        {
            VerifyCorrectParsing(new Dictionary<string, string>
            {
                { "существуют живые композиторы", "(∃ x) x ∈ set<Композитор> & ЖИВОЙ(x)" },
                { "Жанна стара", "СТАРЫЙ(жанна)" },
                { "Облако старо", "СТАРЫЙ(облако)" },
                { "Жанна (-|[-] это) человек", "жанна ∈ set<Человек>" },
                { "Облако (-|[-] это) человек", "облако ∈ set<Человек>" },
                { "Жанна является человеком", "жанна ∈ set<Человек>" },
                { "Каждая зверушка смертна", "(∀ x) x ∈ set<Зверушка> ⇒ СМЕРТНЫЙ(x)" },
                { "Каждое облако конечно", "(∀ x) x ∈ set<Облако> ⇒ КОНЕЧНЫЙ(x)" },
                { "Существует живая рыба", "(∃ x) x ∈ set<Рыба> & ЖИВОЙ(x)" },
                { "Если пациентка жива, то она дышит", "ЖИВОЙ(пациентка) ⇒ ДЫШАТЬ(пациентка)" }
            });
        }

        [TestMethod]
        public void SomeSentencesShouldNotBeParsable()
        {
            foreach (var sentence in new[] 
                {
                    "Сократ стара",
                    "Маша стар",
                    "Каждый облако конечно",
                    "Каждая Сократ смертен",
                    "Каждая зверушка смертен",
                    "Существует живая композитор",
                    "Существует живой рыба"
                })
            {
                Assert.IsNull(GariParser.ParseSentence(sentence));
            }
        }

        [TestMethod]
        public void RepeatedlyParsingTheSameSentenceShouldReturnTheSameResult()
        {
            VerifyCorrectParsing(ParsingSamples.SelectMany(item => new[] { item, item, item }));
        }

        [TestMethod]
        [TestCategory("Helpers")]
        public void DebugSingleParserOnSingleSentence()
        {
            RussianGariParser.PrebuiltParsers["Если пациент жив, то он дышит"].Match(SolarixParserMemoizer.Parse("Если пациент жив, то он дышит").Single());
        }

        [TestMethod]
        [TestCategory("Helpers")]
        public void PrepareParsingExpressionSkeletonsForAllParsingSamples()
        {
            TemporaryParsersWritingHelper.DumpParsingExpressionSkeletons(
                ParsingSamples.Keys.SelectMany(InputStringVariationGenerator.GenerateVariations), 
                SolarixParserMemoizer);
        }

        [ClassInitialize]
        public static void SetupOnce(TestContext unused)
        {
            var solarixRussianGrammarEngine = new SolarixRussianGrammarEngine();
            solarixRussianGrammarEngine.Initialize();
            SolarixParserMemoizer = new SolarixParserMemoizer(solarixRussianGrammarEngine);
            GariParser = new RussianGariParser(SolarixParserMemoizer);
            GariParser.ParsingFailed += TemporaryParsersWritingHelper.DumpParsingExpressionSkeleton;
        }

        [ClassCleanup]
        public static void TeardownOnce()
        {
            SolarixParserMemoizer.Dispose();
        }

        private void VerifyCorrectParsing(IEnumerable<KeyValuePair<string, string>> expectedParsingResults)
        {
            using (var accumulatingAssert = new AccumulatingAssert())
            {
                foreach (var singleSentenceExpectation in expectedParsingResults)
                {
                    foreach (var inputStringVariation in InputStringVariationGenerator.GenerateVariations(singleSentenceExpectation.Key))
                    {
                        var sentenceParsingResult = GariParser.ParseSentence(inputStringVariation);
                        Assert.IsNotNull(sentenceParsingResult, $"{inputStringVariation} failed to be parsed.");
                        accumulatingAssert.AssertEqual(
                            singleSentenceExpectation.Value,
                            sentenceParsingResult.ToString(), 
                            $"Input: {inputStringVariation}");
                    }
                }
            }
        }

        private static SolarixParserMemoizer SolarixParserMemoizer;
        private static RussianGariParser GariParser;

        private static readonly IDictionary<string, string> ParsingSamples =
            new Dictionary<string, string>
            {
              // P(x) 
                { "Сократ стар", "СТАРЫЙ(сократ)" },
                { "Сократ (-|[-] это) человек", "сократ ∈ set<Человек>" },
                { "Сократ является человеком", "сократ ∈ set<Человек>" },
                { "Вода кипит", "КИПЕТЬ(вода)" }, 
              // ¬P(x) 
                { "Сократ не стар", "¬СТАРЫЙ(сократ)" },
                { "Сократ [-] [это] не человек", "¬(сократ ∈ set<Человек>)" },
                { "Сократ не является человеком", "¬(сократ ∈ set<Человек>)" },
                { "Вода не кипит", "¬КИПЕТЬ(вода)" },
                { "неверно, что Сократ стар", "¬СТАРЫЙ(сократ)" },
                { "неверно, что Сократ [-] [это] человек", "¬(сократ ∈ set<Человек>)" },
                { "неверно, что Сократ является человеком", "¬(сократ ∈ set<Человек>)" },
                { "неверно, что вода кипит", "¬КИПЕТЬ(вода)" }, 
              // P(f(x)) 
                { "Отец Сократа стар", "СТАРЫЙ(Отец(сократ))" },
                { "Отец Сократа (-|[-] это) человек", "Отец(сократ) ∈ set<Человек>" },
                { "Отец Сократа является человеком", "Отец(сократ) ∈ set<Человек>" },
                { "Содержимое кастрюли кипит", "КИПЕТЬ(Содержимое(кастрюля))" }, 
              // (∀ x) x ∈ S ⇒ P(x) 
                { "(любой|каждый) человек смертен", "(∀ x) x ∈ set<Человек> ⇒ СМЕРТНЫЙ(x)" },
                { "все люди смертны", "(∀ x) x ∈ set<Люди> ⇒ СМЕРТНЫЙ(x)" },
                { "все лебеди белые", "(∀ x) x ∈ set<Лебедь> ⇒ БЕЛЫЙ(x)" },
              // (∀ x) x ∈ S ⇒ x ∈ T
                { "Лебеди являются птицами", "(∀ x) x ∈ set<Лебедь> ⇒ x ∈ set<Птица>" },
                { "Лебеди[-] это птицы", "(∀ x) x ∈ set<Лебедь> ⇒ x ∈ set<Птица>" },
              // (∃ x) x ∈ S ⇒ P(x) 
                { "существует живой композитор", "(∃ x) x ∈ set<Композитор> & ЖИВОЙ(x)" },
                { "существуют живые композиторы", "(∃ x) x ∈ set<Композитор> & ЖИВОЙ(x)" },
                { "живые композиторы существуют ", "(∃ x) x ∈ set<Композитор> & ЖИВОЙ(x)" }, 
              // P(x, y) 
                { "Вова любит Машу", "ЛЮБИТЬ(вова, маша)" },
                { "Вася выше Пети", "ВЫШЕ(вася, петя)" },
                { "Вася выше, чем Петя", "ВЫШЕ(вася, петя)" }, 
              // P(x) & Q(x) 
                { "Сократ стар и умен", "СТАРЫЙ(сократ) & УМНЫЙ(сократ)" },
                { "Роза цветет и пахнет", "ЦВЕСТИ(роза) & ПАХНУТЬ(роза)" },
                { "Петя (-|[-] это) человек и пароход", "петя ∈ set<Человек> & петя ∈ set<Пароход>" }, 
              // P(x) | Q(x) 
                { "Пациент жив или мертв", "ЖИВОЙ(пациент) | МЕРТВЫЙ(пациент)" }, 
              // P(x) ⇒ Q(x) 
                { "Если пациент жив, то он дышит", "ЖИВОЙ(пациент) ⇒ ДЫШАТЬ(пациент)" },
                { "Из того, что число натуральное, следует, что оно положительное", "НАТУРАЛЬНЫЙ(число) ⇒ ПОЛОЖИТЕЛЬНЫЙ(число)" }, 
              // ¬P(f(x)) 
                { "Отец Сократа не стар", "¬СТАР(Отец(сократ))" },
                { "Отец Сократа [-] [это] не человек", "¬(Отец(сократ) ∈ set<Человек>)" },
                { "Отец Сократа не является человеком", "¬(Отец(сократ) ∈ set<Человек>)" },
                { "Содержимое кастрюли не кипит", "¬КИПИТ(Содержимое(кастрюля))" },
                { "неверно, что отец Сократа стар", "¬СТАР(Отец(сократ))" },
                { "неверно, что отец Сократа [-] [это] человек", "¬(Отец(сократ) ∈ set<Человек>)" },
                { "неверно, что отец Сократа является человеком", "¬(Отец(сократ) ∈ set<Человек>)" },
                { "неверно, что содержимое кастрюли кипит", "¬КИПИТ(Содержимое(кастрюля))" }, 
              // (∀ x) x ∈ S ⇒  P(f(x)) 
                { "Цвет любого лебедя белый", "(∀ x) x ∈ set<Лебедь> ⇒ БЕЛЫЙ(Цвет(x))" },
                { "Все лебеди белого цвета", "(∀ x) x ∈ set<Лебедь> ⇒ БЕЛЫЙ(Цвет(x))" }, 
              // (∃ x) x ∈ S ⇒  P(f(x)) 
                { "существуют лебеди белого цвета", "(∃ x) x ∈ set<Лебедь> & БЕЛЫЙ(Цвет(x))" },
                { "существует кастрюля, содержимое которой кипит", "(∃ x) x ∈ set<Кастрюля> & КИПИТ(Содержимое(x))" },
                { "существуют кастрюли с кипящим содержимым", "(∃ x) x ∈ set<Кастрюля> & КИПИТ(Содержимое(x))" }, 
              // (∀ x) x ∈ S ⇒  ¬P(x) 
                { "(каждый|любой) мужчина не беременен", "(∀ x) x ∈ set<Мужчина> ⇒ ¬БЕРЕМЕНЕН(x)" },
                { "все мужчины не беременны", "(∀ x) x ∈ set<Мужчина> ⇒ ¬БЕРЕМЕНЕН(x)" },
                { "не существует беременных мужчин", "(∀ x) x ∈ set<Мужчина> ⇒ ¬БЕРЕМЕНЕН(x)" },
                { "ни один мужчина не беременен", "(∀ x) x ∈ set<Мужчина> ⇒ ¬БЕРЕМЕНЕН(x)" }, 
              // (∃ x) x ∈ S ⇒  ¬P(x) 
                { "существует не беременная женщина", "(∃ x) x ∈ set<Женщина> & ¬БЕРЕМЕНЕН(x)" },
                { "существуют не беременные женщины", "(∃ x) x ∈ set<Женщина> & ¬БЕРЕМЕНЕН(x)" },
                { "существует женщина, которая не беременна", "(∃ x) x ∈ set<Женщина> & ¬БЕРЕМЕНЕН(x)" }, 
              // ¬P(x, y) 
                { "Вова не любит Машу", "¬ЛЮБИТЬ(вова, маша)" },
                { "Вася не выше Пети", "¬ВЫШЕ(вася, петя)" },
                { "Вася не выше, чем Петя", "¬ВЫШЕ(вася, петя)" }, 
              // P(f(x), y) 
                { "Мама Пети [-] [это] Маша", "\"=\"(Мама(петя), маша)" },
                { "Мамой Пети является Маша", "\"=\"(Мама(петя), маша)" },
                { "Папа Вовы любит Машу", "ЛЮБИТЬ(Папа(вова), маша)" },
                { "Папа Васи выше Пети", "ВЫШЕ(Папа(вася), петя)" },
                { "Папа Васи выше, чем Петя", "ВЫШЕ(Папа(вася), петя)" }, 
              // P(x, f(y)) 
                { "Петя [-] [это] папа Маши", "\"=\"(петя, Папа(маша))" },
                { "Петя является папой Маши", "\"=\"(петя, Папа(маша))" },
                { "Маша любит папу Вовы", "ЛЮБИТЬ(маша, Папа(вова))" },
                { "Петя выше папы Васи ", "ВЫШЕ(петя, Папа(вася))" },
                { "Петя выше, чем папа Васи ", "ВЫШЕ(петя, Папа(вася))" }, 
              // P(f(x), g(y)) 
                { "прибыль Васи [-] [это] убыток Пети", "\"=\"(Прибыль(вася), Убыток(петя))" },
                { "прибыль Васи является убытком Пети", "\"=\"(Прибыль(вася), Убыток(петя))" },
                { "папа Маши любит маму Вовы", "ЛЮБИТЬ(Мама(петя), Папа(вова))" },
                { "мама Пети выше папы Васи ", "ВЫШЕ(Мама(петя), Папа(вася))" },
                { "мама Пети выше, чем папа Васи ", "ВЫШЕ(Мама(петя), Папа(вася))" }, 
              // (∀ x) x ∈ S ⇒  P(x, y) 
                { "(Любой | Каждый) человек знает Адама", "(∀ x) x ∈ set<Человек> ⇒ ЗНАЕТ(x, адам)" },
                { "Все люди знают Адама", "(∀ x) x ∈ set<Человек> ⇒ ЗНАЕТ(x, адам)" }, 
              // (∃ x) x ∈ S ⇒ P(x, y) 
                { "Существует число большее 100", "(∃ x) x ∈ set<Число> & БОЛЬШЕ(x, 100)" },
                { "Существует число, которое больше 100", "(∃ x) x ∈ set<Число> & БОЛЬШЕ(x, 100)" },
                { "(Существуют | Есть) числа, которые больше 100", "(∃ x) x ∈ set<Число> & БОЛЬШЕ(x, 100)" },
                { "Есть люди, которые знают Адама", "(∃ x) x ∈ set<Человек> & ЗНАЕТ(x, адам)" }, 
              // (∀ y) y ∈ S ⇒ P(x, y) 
                { "Петя любит все мультфильмы", "(∀ y) y ∈ set<Мультфильм> ⇒ ЛЮБИТЬ(петя, у)" }, 
              // (∃ y) y ∈ S ⇒ P(x, y) 
                { "?????", "" }, 
              // (∀ x ∀ y) (x ∈ S1 & y ∈ S2) ⇒ P(x, y) 
                { "(Любой|Каждый) ребенок любит [любые | все] конфеты", "(∀ x ∀ y) x ∈ set<Ребенок> & y ∈ set<Конфета> ⇒ ЛЮБИТЬ(x, у)" }, 
              // (∀ x ∃ y) (x ∈ S1 & y ∈ S2) ⇒ P(x, y) 
                { "(Любой|Каждый) ребенок прочел какую-нибудь книгу", "(∀ x ∃ y) x ∈ set<Ребенок> & y ∈ set<Книга> ⇒ ПРОЧЕЛ(x, у)" },
                { "Для (любого | каждого) человека x существует такое животное у, что x это хозяин y", "(∀ x ∃ y) x ∈ set<Человек> & y ∈ set<Животное> ⇒ ХОЗЯИН(x, у)" }, 
              // (∃ x ∀ y) (x ∈ S1 & y ∈ S2) ⇒ P(x, y) 
                { "(Есть | Существует) мультфильм, который нравится всем детям", "(∃ x ∀ y) x ∈ set<Мультфильм> & y ∈ set<Ребенок> ⇒ НРАВИТСЯ(x, у)" }, 
              // (∃ x ∃ y) (x ∈ S1 & y ∈ S2) ⇒ P(x, y) 
                { "????", "" }, 
              // P(x, y, z) 
                { "Петя, Вася и Маша (- | это) друзья", "ДРУЗЬЯ(петя, вася, маша)" },
                { "Петя, Вася и Маша дружат ", "ДРУЗЬЯ(петя, вася, маша)" }, 
              // P(x, y) & Q(v, z) 
                { "Петя любит Машу, (а | и) Витя завидует Кате", "ЛЮБИТЬ(петя, маша) & ЗАВИДУЕТ(витя, катя)" }, 
              // P(x, y) | Q(v, z) 
                { "Или Петя любит Машу, или Витя завидует Кате, или и то и другое", "ЛЮБИТЬ(петя, маша) | ЗАВИДУЕТ(витя, катя)" },
                { "Либо Петя любит Машу, либо Витя завидует Кате, либо и то и другое", "ЛЮБИТЬ(петя, маша) | ЗАВИДУЕТ(витя, катя)" }, 
              // P(x, y) ⇒ Q(v, z) 
                { "Если Петя любит Машу, то Витя завидует Кате", "ЛЮБИТЬ(петя, маша) ⇒ ЗАВИДУЕТ(витя, катя)" },
                { "Из того, что Петя любит Машу, следует, что Витя завидует Кате", "ЛЮБИТЬ(петя, маша) ⇒ ЗАВИДУЕТ(витя, катя)" }, 
              // ¬P(x) & Q(x) 
                { "Сократ не стар (и | , но) умен", "¬СТАР(сократ) & УМЕН(сократ)" }, 
              // ¬P(x) | Q(x) 
                { "Пациент не болен или мертв", "¬БОЛЕН(пациент) | МЕРТВ(пациент)" }, 
              // ¬P(x) ⇒ Q(x) 
                { "Если пациент не болен, то он жив", "¬БОЛЕН(пациент) ⇒ ЖИВ(пациент)" },
                { "Из того, что пациент не болен, следует, что он жив", "¬БОЛЕН(пациент) ⇒ ЖИВ(пациент)" }, 
              // P(x) & ¬Q(x) 
                { "Сократ стар (и | , но) не умен", "СТАР(сократ) & ¬УМЕН(сократ)" }, 
              // P(x) | ¬Q(x) 
                { "Пациент мерв или не болен", "МЕРТВ(пациент) | ¬БОЛЕН(пациент) " }, 
              // P(x) ⇒ ¬Q(x) 
                { "Если пациент жив, то он не болен", "ЖИВ(пациент) ⇒ ¬БОЛЕН(пациент)" },
                { "Из того, что пациент жив, следует, что он не болен", "ЖИВ(пациент) ⇒ ¬БОЛЕН(пациент)" }, 
              // (∀ x) x ∈ S1 ⇒ P(x) & Q(x) 
                { "(Каждый | Любой) философ стар и умен", "(∀ x) x ∈ set<Философ> ⇒ СТАР(x) & УМЕН(x)" },
                { "Все философы стары и умны", "(∀ x) x ∈ set<Философ> ⇒ СТАР(x) & УМЕН(x)" },
                { "Все розы цветут и пахнут", "(∀ x) x ∈ set<Роза> ⇒ ЦВЕТЕТ(x) & ПАХНЕТ(x)" },
                { "Любая роза цветет и пахнет", "(∀ x) x ∈ set<Роза> ⇒ ЦВЕТЕТ(x) & ПАХНЕТ(x)" }, 
              // (∀ x) x ∈ S1 ⇒  P(x) | Q(x) 
                { "(Каждый | Любой) философ стар (или | либо) умен", "(∀ x) x ∈ set<Философ> ⇒ СТАР(x) | УМЕН(x)" },
                { "Все философы стары (или | либо) умны", "(∀ x) x ∈ set<Философ> ⇒ СТАР(x) | УМЕН(x)" },
                { "Все розы цветут (или | либо) пахнут", "(∀ x) x ∈ set<Роза> ⇒ ЦВЕТЕТ(x) | ПАХНЕТ(x)" },
                { "Все розы либо цветут, либо пахнут, либо и то и другое", "(∀ x) x ∈ set<Роза> ⇒ ЦВЕТЕТ(x) | ПАХНЕТ(x)" },
                { "Любая роза цветет (или | либо) пахнет", "(∀ x) x ∈ set<Роза> ⇒ ЦВЕТЕТ(x) | ПАХНЕТ(x)" }, 
              // (∀ x) x ∈ S1 ⇒ P(x) ⇒ Q(x) 
                { "Для любого философа верно, что если он стар, то он умен", "(∀ x) x ∈ set<Философ> ⇒ СТАР(x) ⇒ УМЕН(x)" },
                { "Если [какой-нибудь | некий] философ стар, то он умен", "(∀ x) x ∈ set<Философ> ⇒ СТАР(x) ⇒ УМЕН(x)" }, 
              // (∃ x) x ∈ S1 ⇒ P(x) & Q(x) 
                { "Есть философ, который и стар, и умен", "(∃ x) x ∈ set<Философ> & СТАР(x) & УМЕН(x)" },
                { "Существуют философы, которые и стары и умны одновременно", "(∃ x) x ∈ set<Философ> & СТАР(x) & УМЕН(x)" },
                { "Есть розы, которые цветут и пахнут", "(∃ x) x ∈ set<Роза> & ЦВЕТЕТ(x) & ПАХНЕТ(x)" },
                { "Есть розы, которые и цветут и пахнут одновременно", "(∃ x) x ∈ set<Роза> & ЦВЕТЕТ(x) & ПАХНЕТ(x)" }, 
              // (∃ x) x ∈ S1 ⇒ P(x) | Q(x) 
                { "Существуют розы, которые цветут (или | либо) пахнут", "(∃ x) x ∈ set<Роза> & (ЦВЕТЕТ(x) | ПАХНЕТ(x))" },
                { "Существуют розы, которые либо цветут, либо пахнут, либо и то и другое", "(∃ x) x ∈ set<Роза> & (ЦВЕТЕТ(x) | ПАХНЕТ(x))" }, 
              // (∃ x) x ∈ S1 ⇒ (P(x) ⇒ Q(x)) 
                { "Существуют розы, которые пахнут если цветут", "(∃ x) x ∈ set<Роза> & (ЦВЕТЕТ(x) ⇒ ПАХНЕТ(x))" },
            };
    }
}
