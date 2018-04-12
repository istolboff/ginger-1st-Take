using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class LogicQuantifier : LogicFormula, IEquatable<LogicQuantifier>
    {
        public LogicQuantifier(QuantifierType type, LogicVariable variable, LogicFormula scope)
        {
            Type = type;
            Variable = variable;
            Scope = scope;
        }

        public QuantifierType Type { get; }

        public LogicVariable Variable { get; }

        public LogicFormula Scope { get; }

        public bool Equals(LogicQuantifier other) =>
            other != null &&
            other.Type == Type &&
            other.Variable.Equals(Variable) &&
            other.Scope.Equals(Scope);

        public override bool Equals(LogicFormula other) => Equals(other as LogicQuantifier);

        public override bool Equals(object obj) => Equals(obj as LogicQuantifier);

        public override int GetHashCode()
        {
            unchecked
            {
                return (((Type.GetHashCode() * 397) ^ Variable.GetHashCode()) * 397) ^ Scope.GetHashCode();
            }
        }

        public override string ToString() => $"({(Type == QuantifierType.Universal ? '∀' : '∃')} {Variable}) {Scope}";
    }
}
