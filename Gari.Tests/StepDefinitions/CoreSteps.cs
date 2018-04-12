using System.Linq;
using TechTalk.SpecFlow;
using Gari.Tests.StepDefinitions.DomainObjects;
using Gari.Tests.Utilities;

namespace Gari.Tests.StepDefinitions
{
    [Binding]
    public static class CoreSteps
    {
        [Given(@"SUT is expected to obey the following rules")]
        public static void SutIsExpectedToObeyTheFollowingRules(Table table)
        {
            Sut.ObeysRules(table.Rows.Select(row => TestRun.GariParser.ParseSentence(row["Rule"])));
        }

        [Then(@"Falsifying Scenario should be generated")]
        public static void FalsifyingScenarioShouldBeGenerated(Table table)
        {
            var problemsLog = new ProblemsLog();
            var falsifyingScenarios = Sut.GenerateFalsifyingScenarios(problemsLog);
            using (var accumulatingAssert = new AccumulatingAssert())
            {
                foreach (var expectedScenario in table.Rows.Select(row =>
                    new
                    {
                        UserAction = row["User action"],
                        ExpectedOutcome = row["Expected Outcome"]
                    }))
                {
                    accumulatingAssert.AssertIsTrue(
                        falsifyingScenarios.Any(scenario =>
                            expectedScenario.UserAction.Equals(scenario.Stimulus.ToString()) &&
                            expectedScenario.ExpectedOutcome.Equals(scenario.ExpectedOutcome.ToString())),
                        $"Scenario '{expectedScenario.UserAction} => {expectedScenario.ExpectedOutcome}' is missing.");
                }

                accumulatingAssert.AssertIsTrue(
                    problemsLog.IsEmpty,
                    problemsLog.ToString());

                accumulatingAssert.AssertEqual(table.Rows.Count, falsifyingScenarios.Count, "wrong number of fasifying scenarions is generated.");
            }
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
