namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class InfinitiveCharacteristics : GrammarCharacteristics
    {
        public InfinitiveCharacteristics(VerbAspect verbAspect, Transitiveness transitiveness, string perfectForm)
        {
            VerbAspect = verbAspect;
            Transitiveness = transitiveness;
            PerfectForm = perfectForm;
        }

        public VerbAspect VerbAspect { get; }

        public Transitiveness Transitiveness { get; }

        public string PerfectForm { get; private set; }

        public override string ToString() => $"Вид={VerbAspect}; Переходность={Transitiveness}";
    }
}
