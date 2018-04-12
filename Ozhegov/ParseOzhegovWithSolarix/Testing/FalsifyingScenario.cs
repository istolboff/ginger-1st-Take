using ParseOzhegovWithSolarix.PredicateLogic;

namespace ParseOzhegovWithSolarix.Testing
{
    public sealed class FalsifyingScenario
    {
        public FalsifyingScenario(LogicFunction stimulus, LogicFormula expectedOutcome)
        {
            Stimulus = stimulus;
            ExpectedOutcome = expectedOutcome;
        }

        public LogicFunction Stimulus { get; }

        public LogicFormula ExpectedOutcome { get; }
    }
}
