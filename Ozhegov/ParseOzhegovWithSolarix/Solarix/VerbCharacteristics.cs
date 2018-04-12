namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class VerbCharacteristics : GrammarCharacteristics
    {
        public VerbCharacteristics(
            Case? @case, 
            Number number, 
            VerbForm verbForm, 
            Person? person, 
            VerbAspect verbAspect, 
            Tense tense,
            Transitiveness? transitiveness)
        {
            Case = @case;
            Number = number;
            VerbForm = verbForm;
            Person = person;
            VerbAspect = verbAspect;
            Tense = tense;
            Transitiveness = transitiveness;
        }

        public Case? Case { get; }

        public Number Number { get; }

        public VerbForm VerbForm { get; }

        public Person? Person { get; }

        public VerbAspect VerbAspect { get; }

        public Tense Tense { get; }

        public Transitiveness? Transitiveness { get; }

        public override string ToString()
        {
            return $"Падеж={Case}; Число={Number}; Наклонение={VerbForm}; Лицо={Person}; Вид={VerbAspect}; Время={Tense}; Переходность={Transitiveness}";
        }
    }
}
