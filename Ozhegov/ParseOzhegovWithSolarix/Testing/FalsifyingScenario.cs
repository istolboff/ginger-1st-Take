using System.Collections.Generic;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.PredicateLogic;

namespace ParseOzhegovWithSolarix.Testing
{
    public sealed class FalsifyingScenario
    {
        public FalsifyingScenario(IEnumerable<LogicFunction> stimuli, LogicFormula expectedOutcome)
        {
            Stimuli = stimuli.AsImmutable();
            ExpectedOutcome = expectedOutcome;
        }

        public IReadOnlyCollection<LogicFunction> Stimuli { get; }

        public LogicFormula ExpectedOutcome { get; }
    }
}
