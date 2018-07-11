using System.Linq;
using TechTalk.SpecFlow;
using Gari.Tests.StepDefinitions.DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParseOzhegovWithSolarix.Miscellaneous;

namespace Gari.Tests.StepDefinitions
{
    [Binding]
    public static class CoreSteps
    {
        [Given(@"Поведение тестируемой системы описывается правилами")]
        public static void SutIsExpectedToObeyTheFollowingRules(Table table)
        {
            Sut.ObeysRules(table.Rows.Select(row => TestRun.GariParser.ParseSentence(row["Rule"])));
        }

        [Given(@"определены следующие воздействия на систему")]
        public static void StimuliDefined(Table table)
        {
            // Sut.KnowsStimuli(table.Rows.Select(row => TestRun.GariParser.ParseSentence(row["Rule"])));
            ScenarioContext.Current.Pending();
        }

        [Then(@"для поверки ожидаемой реакции '(.*)' будет сгенерирована последовательность воздействий на систему")]
        public static void FalsifyingScenarioShouldBeGenerated(string expectedReaction, Table expectedStimuliTable)
        {
            var problemsLog = new ProblemsLog();
            var falsifyingScenarios = Sut.GenerateFalsifyingScenarios(problemsLog);
            Assert.IsTrue(problemsLog.IsEmpty, problemsLog.ToString());

            var expectedStimuli = expectedStimuliTable.Rows.Select(row => TestRun.GariParser.ParseSentence(row["Воздействие на систему"])).AsImmutable();
            // Assert.IsTrue(falsifyingScenarios.Any(scenario => scenario.Stimuli.SequenceEqual(expectedStimuli)));
            ScenarioContext.Current.Pending();
        }

        internal static SystemUnderTest Sut
        {
            get
            {
                return TestRun.EstablishLazyReadOnlyProperty(() => new SystemUnderTest(TestRun.RussianGrammarEngine));
            }
        }
    }
}
