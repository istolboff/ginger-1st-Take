using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class LogicVariable : LogicTerm, IEquatable<LogicVariable>
    {
        public LogicVariable(string variableName)
        {
            Name = variableName;
        }

        public string Name { get; }

        public bool Equals(LogicVariable other) => other != null && Name == other.Name;

        public override bool Equals(LogicTerm other) => Equals(other as LogicVariable);

        public override bool Equals(object obj) => Equals(obj as LogicVariable);

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => Name.ToLower();
    }
}
