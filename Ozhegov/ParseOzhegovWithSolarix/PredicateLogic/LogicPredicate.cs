using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public class LogicPredicate : IFormattable, IEquatable<LogicPredicate>
    {
        public LogicPredicate(string predicateName, params LogicTerm[] logicTerms)
        {
            Name = predicateName;
            LogicTerms = new ReadOnlyCollection<LogicTerm>(logicTerms);
        }

        public string Name { get; }

        public ReadOnlyCollection<LogicTerm> LogicTerms { get; }

        public bool Equals(LogicPredicate other) =>
            other != null &&
            other.GetType() == typeof(LogicPredicate) &&
            other.Name == Name &&
            other.LogicTerms.SequenceEqual(LogicTerms);

        public override bool Equals(object obj) => Equals(obj as LogicPredicate);

        public override int GetHashCode() => (Name.GetHashCode() * 397) ^ LogicTerms.Count.GetHashCode();

        public override string ToString() => $"{Name.ToUpper()}({string.Join(", ", LogicTerms)})";

        public virtual string ToString(string format, IFormatProvider formatProvider) => ToString();
    }
}
