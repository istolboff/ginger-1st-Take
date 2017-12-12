using System;
using System.Linq;
using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal sealed class PartOfSpeechMatcher : SentenceElementMatcherBase<PartOfSpeechMatcher>
    {
        public PartOfSpeechMatcher(
            Func<SentenceElement, SentenceElement> getElementToMatch, 
            PartOfSpeech expectedPartOfSpeech, 
            string expectedContent)
            : base(getElementToMatch)
        {
            _expectedPartOfSpeech = expectedPartOfSpeech;
            _expectedContent = expectedContent;
        }

        public object Detected { get; internal set; }

        protected override LemmaVersion MatchCore(SentenceElement elementToMatch)
        {
            return !_expectedContent.Equals(elementToMatch.Content, StringComparison.InvariantCultureIgnoreCase)
                ? null
                : elementToMatch.LemmaVersions.FirstOrDefault(lemmaVersion => lemmaVersion.PartOfSpeech == _expectedPartOfSpeech);
        }

        protected override PartOfSpeechMatcher This => this;

        private readonly PartOfSpeech _expectedPartOfSpeech;
        private readonly string _expectedContent;
    }
}
