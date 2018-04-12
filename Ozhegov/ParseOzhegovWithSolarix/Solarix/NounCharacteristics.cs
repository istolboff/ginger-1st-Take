namespace ParseOzhegovWithSolarix.Solarix
{
    public class NounCharacteristics : GrammarCharacteristics
    {
        public NounCharacteristics(Case @case, Number number, Gender gender, Form? form)
        {
            Case = @case;
            Number = number;
            Gender = gender;
            Form = form;
        }

        public Case Case { get; }

        public Number Number { get; }

        public Gender Gender { get; }

        public Form? Form { get; }

        public override string ToString()
        {
            return $"Падеж={Case}; Число={Number};Род={Gender};Одушевленность={Form}";
        }
    }
}
