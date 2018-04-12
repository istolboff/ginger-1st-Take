namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class VerbalNounCharacteristics : NounCharacteristics
    {
        public VerbalNounCharacteristics(Case @case, Number number, Gender gender, Form? form, string relatedInfinitive)
            : base(@case, number, gender, form)
        {
            RelatedInfinitive = relatedInfinitive;
        }

        public string RelatedInfinitive { get; }
    }
}
