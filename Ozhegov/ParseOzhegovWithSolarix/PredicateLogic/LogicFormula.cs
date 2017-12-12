using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public abstract class LogicFormula : IEquatable<LogicFormula>
    {
        public abstract bool Equals(LogicFormula other);

        public LogicFormula Follows(LogicFormula followsThat)
        {
            return new LogicConnective(LogicConnectiveType.Follows, this, followsThat);
        }

        public static LogicFormula operator &(LogicFormula left, LogicFormula right)
        {
            return new LogicConnective(LogicConnectiveType.And, left, right);
        }
    }
}
