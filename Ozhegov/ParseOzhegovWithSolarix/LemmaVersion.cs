namespace ParseOzhegovWithSolarix
{
    public sealed class LemmaVersion
    {
        public LemmaVersion(string lemma, PartOfSpeech? partOfSpeech)
        {
            Lemma = lemma;
            PartOfSpeech = partOfSpeech;
        }

        public string Lemma { get; }

        public PartOfSpeech? PartOfSpeech { get; }
    }
}
