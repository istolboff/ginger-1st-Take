using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class SetContainsPredicate : LogicPredicate, IEquatable<SetContainsPredicate>
    {
        public SetContainsPredicate(string setName, string objectName)
            : base("∈", new LogicConstant(setName), new LogicVariable(objectName))
        {
        }

        public string SetName => LogicTerms[0].ToString();

        public string ObjectName => LogicTerms[1].ToString();

        public bool Equals(SetContainsPredicate other) => 
            other != null &&
            other.SetName == SetName &&
            other.ObjectName == ObjectName;

        public override string ToString() => $"{ObjectName} ∈ set<{SetName}>";

        public override string ToString(string format, IFormatProvider formatProvider) => 
            format == "N" ? $"({ToString()})" : ToString();
    }
}
