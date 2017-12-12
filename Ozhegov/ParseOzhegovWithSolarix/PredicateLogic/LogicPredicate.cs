using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public class LogicPredicate : LogicFormula, IFormattable, IEquatable<LogicPredicate>
    {
        public LogicPredicate(string predicateName, params LogicTerm[] terms)
        {
            Name = predicateName;
            Terms = new ReadOnlyCollection<LogicTerm>(terms);
        }

        public string Name { get; }

        public ReadOnlyCollection<LogicTerm> Terms { get; }

        public virtual bool Equals(LogicPredicate other) =>
            other != null &&
            other.GetType() == typeof(LogicPredicate) &&
            other.Name == Name &&
            other.Terms.SequenceEqual(Terms);

        public override bool Equals(LogicFormula other) => Equals(other as LogicPredicate);

        public override bool Equals(object obj) => Equals(obj as LogicPredicate);

        public override int GetHashCode() => (Name.GetHashCode() * 397) ^ Terms.Count.GetHashCode();

        public override string ToString() => $"{Name.ToUpper()}({string.Join(", ", Terms)})";

        public virtual string ToString(string format, IFormatProvider formatProvider) => ToString();
    }
}
