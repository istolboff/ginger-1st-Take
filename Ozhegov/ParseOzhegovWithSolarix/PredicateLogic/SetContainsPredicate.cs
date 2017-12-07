using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class SetContainsPredicate : LogicPredicate, IEquatable<SetContainsPredicate>
    {
        public SetContainsPredicate(string setName, LogicTerm setElement)
            : base("∈", new LogicConstant(setName), setElement)
        {
        }

        public string SetName => Terms[0].ToString();

        public string SetElement => Terms[1].ToString();

        public bool Equals(SetContainsPredicate other) => 
            other != null &&
            other.SetName == SetName &&
            other.SetElement == SetElement;

        public override string ToString() => $"{SetElement} ∈ set<{SetName}>";

        public override string ToString(string format, IFormatProvider formatProvider) => 
            format == "N" ? $"({ToString()})" : ToString();
    }
}
