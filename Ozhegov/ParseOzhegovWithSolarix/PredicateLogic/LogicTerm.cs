using System;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public abstract class LogicTerm : IEquatable<LogicTerm>
    {
        public abstract bool Equals(LogicTerm other);
    }
}
