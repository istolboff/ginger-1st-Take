namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class NegatedPredicate : LogicPredicate
    {
        public NegatedPredicate(LogicPredicate target)
            : base("¬")
        {
            Target = target;
        }

        public LogicPredicate Target { get; }

        public override string ToString() => $"¬{Target}";
    }
}
