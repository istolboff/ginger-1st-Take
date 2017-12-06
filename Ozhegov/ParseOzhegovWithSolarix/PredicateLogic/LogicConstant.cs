using ParseOzhegovWithSolarix.Miscellaneous;
using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class LogicConstant : LogicTerm, IEquatable<LogicConstant>
    {
        public LogicConstant(string constantName)
        {
            Name = constantName;
        }

        public string Name { get; }

        public bool Equals(LogicConstant other) => other != null && Name == other.Name;

        public override bool Equals(LogicTerm other) => Equals(other as LogicConstant);

        public override bool Equals(object obj) => Equals(obj as LogicConstant);

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => Name.FirstCharToUpper();
    }
}
