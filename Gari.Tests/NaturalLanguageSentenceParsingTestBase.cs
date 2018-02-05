using Gari.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParseOzhegovWithSolarix;
using ParseOzhegovWithSolarix.Solarix;

namespace Gari.Tests
{
    [TestClass]
    public abstract class NaturalLanguageSentenceParsingTestBase
    {
        protected static void SetupOnceCore()
        {
            var solarixRussianGrammarEngine = new SolarixRussianGrammarEngine();
            solarixRussianGrammarEngine.Initialize();
            SolarixParserMemoizer = new SolarixParserMemoizer(solarixRussianGrammarEngine);
            GariParser = new RussianGariParser(SolarixParserMemoizer);
            GariParser.ParsingFailed += TemporaryParsersWritingHelper.DumpParsingExpressionSkeleton;
        }

        protected static void TeardownOnceCore()
        {
            SolarixParserMemoizer.Dispose();
        }

        protected static SolarixParserMemoizer SolarixParserMemoizer;
        protected static RussianGariParser GariParser;
    }
}
