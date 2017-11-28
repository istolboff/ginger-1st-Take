using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix.SentenceStructureRecognizing
{
    internal interface ISentenceElementMatcher<out TResult>
    {
        IOptional<TResult> Match(SentenceElement rootSentenceElement);
    }
}
