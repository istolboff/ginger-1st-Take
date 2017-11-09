namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class AdverbCharacteristics : GrammarCharacteristics
    {
        public AdverbCharacteristics(ComparisonForm comparisonForm)
        {
            ComparisonForm = comparisonForm;
        }

        public ComparisonForm ComparisonForm { get; }

        public override string ToString()
        {
            return $"Степень={ComparisonForm}";
        }
    }
}
