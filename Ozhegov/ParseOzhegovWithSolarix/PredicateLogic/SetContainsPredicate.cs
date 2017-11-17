namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class SetContainsPredicate : LogicPredicate
    {
        public SetContainsPredicate(string setName, string objectName)
            : base("∈", new LogicVariable(setName), new LogicVariable(objectName))
        {
            SetName = setName;
            ObjectName = objectName;
        }

        public string SetName { get; }

        public string ObjectName { get; }

        public override string ToString()
        {
            return $"{ObjectName} ∈ set<{SetName}>";
        }
    }
}
