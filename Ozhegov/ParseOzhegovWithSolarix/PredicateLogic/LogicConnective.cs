using System;
using System.Collections.Generic;

namespace ParseOzhegovWithSolarix.PredicateLogic
{
    public class LogicConnective : LogicFormula, IEquatable<LogicConnective>
    {
        public LogicConnective(LogicConnectiveType connectiveType, LogicFormula leftFormula, LogicFormula rightFormula)
        {
            ConnectiveType = connectiveType;
            LeftFormula = leftFormula;
            RightFormula = rightFormula;
        }

        public LogicConnectiveType ConnectiveType { get; }

        public LogicFormula LeftFormula { get; }

        public LogicFormula RightFormula { get; }

        public bool Equals(LogicConnective other) =>
            other != null &&
            other.ConnectiveType == ConnectiveType &&
            other.LeftFormula.Equals(LeftFormula) &&
            other.RightFormula.Equals(RightFormula);

        public override bool Equals(LogicFormula other) => Equals(other as LogicConnective);

        public override bool Equals(object obj) => Equals(obj as LogicConnective);

        public override int GetHashCode()
        {
            unchecked
            {
                return (((ConnectiveType.GetHashCode() * 397) ^ LeftFormula.GetHashCode()) * 397) ^ RightFormula.GetHashCode();
            }
        }

        public override string ToString() => $"{LeftFormula} {ConnectiveTypeCharacter[ConnectiveType]} {RightFormula}";

        private static readonly IDictionary<LogicConnectiveType, char> ConnectiveTypeCharacter = 
            new Dictionary<LogicConnectiveType, char>
            {
                { LogicConnectiveType.Follows, '⇒' },
                { LogicConnectiveType.And, '&' },
                { LogicConnectiveType.Or, '|' }
            };
    }
}
