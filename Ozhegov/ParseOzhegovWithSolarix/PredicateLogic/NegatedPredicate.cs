using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class NegatedPredicate : LogicPredicate, IEquatable<NegatedPredicate>
    {
        public NegatedPredicate(LogicPredicate target)
            : base("¬")
        {
            Target = target;
        }

        public LogicPredicate Target { get; }

        public bool Equals(NegatedPredicate other) => other != null && other.Target.Equals(Target);

        public override bool Equals(object obj) => Equals(obj as NegatedPredicate);

        public override int GetHashCode() => (Target.GetHashCode() * 397) ^ 1;

        public override string ToString() => $"¬{Target.ToString("N", null)}";
    }
}
