using ParseOzhegovWithSolarix.PredicateLogic;

namespace ParseOzhegovWithSolarix.Planning
{
    public sealed class ActionWithConsequence : LogicConnective
    {
        public ActionWithConsequence(LogicFormula leftFormula, LogicFormula rightFormula) 
            : base(LogicConnectiveType.Follows, leftFormula, rightFormula)
        {
        }
    }
}
