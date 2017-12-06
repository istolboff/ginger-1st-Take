namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class LemmaVersion
    {
        public LemmaVersion(string lemma, PartOfSpeech? partOfSpeech, GrammarCharacteristics characteristics)
        {
            Lemma = lemma;
            PartOfSpeech = partOfSpeech;
            Characteristics = characteristics;
        }

        public string Lemma { get; }

        public PartOfSpeech? PartOfSpeech { get; }

        public GrammarCharacteristics Characteristics { get; }

        public override string ToString()
        {
            return $"{PartOfSpeech} [{Characteristics}]";
        }
    }
}
