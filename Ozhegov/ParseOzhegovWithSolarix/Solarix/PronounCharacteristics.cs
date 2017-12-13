namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class PronounCharacteristics : GrammarCharacteristics
    {
        public PronounCharacteristics(Gender gender, Number number, Person person)
        {
            Gender = gender;
            Number = number;
            Person = person;
        }

        public Gender Gender { get; }

        public Number Number { get; }

        public Person Person { get; }

        public override string ToString() => $"Род={Gender}; Число={Number}; Лицо={Person}";
    }
}
