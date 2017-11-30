using System;
using System.Collections.ObjectModel;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public class LogicPredicate : IFormattable
    {
        public LogicPredicate(string predicateName, params LogicTerm[] logicTerms)
        {
            Name = predicateName;
            LogicTerms = new ReadOnlyCollection<LogicTerm>(logicTerms);
        }

        public string Name { get; }

        public ReadOnlyCollection<LogicTerm> LogicTerms { get; }

        public override string ToString() => $"{Name.ToUpper()}({string.Join(", ", LogicTerms)})";

        public virtual string ToString(string format, IFormatProvider formatProvider) => ToString();
    }
}
