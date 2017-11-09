namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class LemmaVersion
    {
        public LemmaVersion(string lemma, PartOfSpeech? partOfSpeech, GrammarCharacteristics grammarCharacteristcs)
        {
            Lemma = lemma;
            PartOfSpeech = partOfSpeech;
            Characteristics = grammarCharacteristcs;
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
