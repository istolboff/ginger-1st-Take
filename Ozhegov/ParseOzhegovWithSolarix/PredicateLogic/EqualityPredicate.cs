using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class EqualityPredicate : LogicPredicate, IEquatable<EqualityPredicate>
    {
        public EqualityPredicate(LogicTerm leftTerm, LogicTerm rightTerm)
            : base(Symbol, leftTerm, rightTerm)
        {
        }

        public LogicTerm LeftTerm => Terms[0];

        public LogicTerm RightTerm => Terms[1];

        public bool Equals(EqualityPredicate other) =>
            other != null &&
            other.LeftTerm.Equals(LeftTerm) &&
            other.RightTerm.Equals(RightTerm);

        public override bool Equals(LogicPredicate other) => Equals(other as EqualityPredicate);

        public override bool Equals(LogicFormula other) => Equals(other as EqualityPredicate);

        public override bool Equals(object obj) => Equals(obj as EqualityPredicate);

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"{LeftTerm} = {RightTerm}";

        public override string ToString(string format, IFormatProvider formatProvider) =>
            format == "N" ? $"({ToString()})" : ToString();

        public static readonly string Symbol = "=";
    }
}
