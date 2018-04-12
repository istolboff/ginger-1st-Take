using System;
using System.Collections.Generic;
using System.Linq;
using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Planning;
using ParseOzhegovWithSolarix.PredicateLogic;
using ParseOzhegovWithSolarix.Solarix;
using ParseOzhegovWithSolarix.Testing;

namespace Gari.Tests.StepDefinitions.DomainObjects
{
    internal class SystemUnderTest
    {
        public SystemUnderTest(IThesaurus thesaurus)
        {
            _thesaurus = thesaurus;
        }

        public void ObeysRules(IEnumerable<LogicFormula> rules)
        {
            _rules.AddRange(rules);
        }

        public IReadOnlyCollection<FalsifyingScenario> GenerateFalsifyingScenarios(ProblemsLog problemsLog)
        {
            return _rules
                    .OfType<ActionWithConsequence>()
                    .Select(actionWithConsequence => 
                        new
                        {
                            actionWithConsequence.Action,
                            ActionOutcome = TryToDeduceFunctionOutcome(actionWithConsequence.Consequence, problemsLog)
                        })
                    .Where(item => item.ActionOutcome.HasValue)
                    .Select(item => 
                        new FalsifyingScenario(
                            item.Action,
                            item.ActionOutcome.Value))
                    .AsImmutable();
        }

        private IOptional<LogicFormula> TryToDeduceFunctionOutcome(LogicFunction actionConsequence, ProblemsLog problemsLog)
        {
            var result = _thesaurus
                    .FindLinkedParticipleInPassiveVoice(actionConsequence.Name)
                    .Map(participle => new LogicPredicate(participle, actionConsequence.Arguments));
            if (!result.HasValue)
            {
                problemsLog.CouldNotDeduceFunctionOutcome(actionConsequence);
            }

            return result;
        }

        private readonly List<LogicFormula> _rules = new List<LogicFormula>();
        private readonly IThesaurus _thesaurus;
    }

    public sealed class ProblemsLog
    {
        public bool IsEmpty
        {
            get { return !_problems.Any(); }
        }

        public void CouldNotDeduceFunctionOutcome(LogicFunction actionConsequence)
        {
            _problems.Add($"Couldn't deduce outcome for function '{actionConsequence.Name}'");
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _problems);
        }

        private readonly List<string> _problems = new List<string>();
    }
}
