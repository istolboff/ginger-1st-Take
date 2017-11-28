using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal struct ElementMatchingResult
    {
        public ElementMatchingResult(string content, LemmaVersion lemmaVersion)
        {
            Content = content;
            LemmaVersion = lemmaVersion;
        }

        public string Content { get; }

        public LemmaVersion LemmaVersion { get; }
    }
}
