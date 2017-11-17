using System.Collections.Generic;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public class LogicPredicate
    {
        public LogicPredicate(string predicateName, params LogicTerm[] logicTerms)
        {
            Name = predicateName;
            LogicTerms = logicTerms;
        }

        public string Name { get; }

        public IReadOnlyCollection<LogicTerm> LogicTerms { get; }

        public override string ToString()
        {
            return $"{Name.ToUpper()}({string.Join(", ", LogicTerms)})";
        }
    }
}
