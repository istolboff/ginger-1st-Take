namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class AdjectiveCharacteristics : GrammarCharacteristics
    {
        public AdjectiveCharacteristics(Case? @case, Number? number, Gender? gender, AdjectiveForm adjectiveForm, ComparisonForm comparisonForm)
        {
            Case = @case;
            Number = number;
            Gender = gender;
            AdjectiveForm = adjectiveForm;
            ComparisonForm = comparisonForm;
        }

        public Case? Case { get; }

        public Number? Number { get; }

        public Gender? Gender { get; }

        public AdjectiveForm AdjectiveForm { get; }

        public ComparisonForm ComparisonForm { get; }

        public override string ToString()
        {
            return $"Падеж={Case}; Число={Number}; Род={Gender}; Форма={AdjectiveForm}; Степень={ComparisonForm}";
        }
    }
}
