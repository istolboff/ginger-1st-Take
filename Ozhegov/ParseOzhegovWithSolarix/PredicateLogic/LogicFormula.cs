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

        public static LogicFormula operator |(LogicFormula left, LogicFormula right)
        {
            return new LogicConnective(LogicConnectiveType.Or, left, right);
        }

        public static LogicFormula Negate(LogicFormula logicFormula)
        {
            switch (logicFormula)
            {
                case NegatedPredicate negatedPredicate:
                    return negatedPredicate.Target;

                case LogicPredicate predicate:
                    return new NegatedPredicate(predicate);

                case LogicConnective connective:
                    switch (connective.ConnectiveType)
                    {
                        case LogicConnectiveType.And:
                            switch (connective.LeftFormula)
                            {
                                case SetContainsPredicate setContains:
                                    return new LogicConnective(LogicConnectiveType.Follows, connective.LeftFormula, Negate(connective.RightFormula));

                                default:
                                    return new LogicConnective(LogicConnectiveType.Or, Negate(connective.LeftFormula), Negate(connective.RightFormula));
                            }

                        case LogicConnectiveType.Or:
                            return new LogicConnective(LogicConnectiveType.And, Negate(connective.LeftFormula), Negate(connective.RightFormula));

                        case LogicConnectiveType.Follows:
                            return new LogicConnective(LogicConnectiveType.And, connective.LeftFormula, Negate(connective.RightFormula));

                        default:
                            throw new NotSupportedException(connective.ConnectiveType.ToString());
                    }

                case LogicQuantifier quantifier:
                    return new LogicQuantifier(
                        quantifier.Type == QuantifierType.Universal ? QuantifierType.Existential : QuantifierType.Universal,
                        quantifier.Variable,
                        Negate(quantifier.Scope));

                default:
                    throw new NotImplementedException($"Negating {logicFormula.GetType()}");
            }
        }
    }
}
