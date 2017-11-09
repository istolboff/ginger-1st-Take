namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class GerundCharacteristics : GrammarCharacteristics
    {
        public GerundCharacteristics(Case @case, VerbAspect verbAspect)
        {
            Case = @case;
            VerbAspect = verbAspect;
        }

        public Case Case { get; }

        public VerbAspect VerbAspect { get; }

        public override string ToString()
        {
            return $"Падеж={Case}; Вид={VerbAspect}";
        }
    }
}
