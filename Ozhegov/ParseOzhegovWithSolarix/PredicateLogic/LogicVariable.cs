namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class LogicVariable : LogicTerm
    {
        public LogicVariable(string variableName)
        {
            Name = variableName;
        }

        public string Name { get; }

        public override string ToString() => Name.ToLower();
    }
}
