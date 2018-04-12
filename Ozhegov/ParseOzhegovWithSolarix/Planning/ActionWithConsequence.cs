using System;
using ParseOzhegovWithSolarix.PredicateLogic;

namespace ParseOzhegovWithSolarix.Planning
{
    public sealed class ActionWithConsequence : LogicFormula, IEquatable<ActionWithConsequence>
    {
        public ActionWithConsequence(LogicFunction action, LogicFunction consequence) 
        {
            Action = action;
            Consequence = consequence;
        }

        public LogicFunction Action { get; }

        public LogicFunction Consequence { get; }

        public bool Equals(ActionWithConsequence other)
        {
            return other != null &&
                other.Action.Equals(Action) &&
                other.Consequence.Equals(Consequence);
        }

        public override bool Equals(LogicFormula other)
        {
            return Equals(other as ActionWithConsequence);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Action.GetHashCode() * 397) ^ Consequence.GetHashCode();
            }
        }
    }
}
