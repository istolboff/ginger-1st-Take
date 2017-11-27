using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public sealed class LogicConstant : LogicTerm
    {
        public LogicConstant(string constantName)
        {
            Name = constantName;
        }

        public string Name { get; }

        public override string ToString() => Name.FirstCharToUpper();
    }
}
