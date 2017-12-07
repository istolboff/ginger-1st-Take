using System;
using System.Collections.ObjectModel;
using System.Linq;
using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class LogicFunction : LogicTerm, IEquatable<LogicFunction>
    {
        public LogicFunction(string name, params LogicTerm[] arguments)
        {
            Name = name;
            Arguments = new ReadOnlyCollection<LogicTerm>(arguments);
        }

        public string Name { get; }

        public ReadOnlyCollection<LogicTerm> Arguments { get; }

        public bool Equals(LogicFunction other) =>
            other != null &&
            other.Name == Name &&
            other.Arguments.SequenceEqual(Arguments);

        public override bool Equals(LogicTerm other) => Equals(other as LogicFunction);

        public override bool Equals(object obj) => Equals(obj as LogicFunction);

        public override int GetHashCode() => (Name.GetHashCode() * 397) ^ Arguments.Count.GetHashCode();

        public override string ToString() => $"{Name.FirstCharToUpper()}({string.Join(", ", Arguments)})";
    }
}
