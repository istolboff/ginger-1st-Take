using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParseOzhegovWithSolarix;
using ParseOzhegovWithSolarix.Solarix;
using Gari.Tests.Utilities;

namespace Gari.Tests
{
    [TestClass]
    public abstract class NaturalLanguageSentenceParsingTestBase
    {
        protected static void SetupOnceCore()
        {
            RussianGrammarEngine = new SolarixRussianGrammarEngine();
            RussianGrammarEngine.Initialize();
            SolarixParserMemoizer = new SolarixParserMemoizer(RussianGrammarEngine);
            GariParser = new RussianGariParser(SolarixParserMemoizer);
            GariParser.ParsingFailed += TemporaryParsersWritingHelper.DumpParsingExpressionSkeleton;
        }

        protected static void TeardownOnceCore()
        {
            SolarixParserMemoizer.Dispose();
        }

        public static SolarixRussianGrammarEngine RussianGrammarEngine { get; private set; }

        public static RussianGariParser GariParser { get; private set; }

        protected static SolarixParserMemoizer SolarixParserMemoizer { get; private set; }
    }
}
