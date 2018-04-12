using Gari.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParseOzhegovWithSolarix.PredicateLogic;
using System.Collections.Generic;

namespace Gari.Tests
{
    [TestClass]
    public sealed class LogicFormulaNegationTests : NaturalLanguageSentenceParsingTestBase
    {
        [TestMethod]
        public void LogicFormulaNegationMainCases()
        {
            foreach (var formula in new Dictionary<string, string>
                {
                    { "Сократ стар", "¬СТАРЫЙ(сократ)" },
                    { "Отец Сократа стар", "¬СТАРЫЙ(Отец(сократ))" },
                    { "все лебеди белые", "(∃ x) x ∈ set<Лебедь> & ¬БЕЛЫЙ(x)" },
                    { "Каждый лебедь это птица", "(∃ x) x ∈ set<Лебедь> & ¬(x ∈ set<Птица>)" },
                    { "существует живой композитор", "(∀ x) x ∈ set<Композитор> ⇒ ¬ЖИВОЙ(x)" },
                    { "Сократ стар и умен", "¬СТАРЫЙ(сократ) | ¬УМНЫЙ(сократ)" },
                    { "Если Сократ жив, то он дышит", "ЖИВОЙ(сократ) & ¬ДЫШАТЬ(сократ)" },
                    { "Каждый ребенок любит любые конфеты", "(∃ x ∃ y) x ∈ set<Ребенок> & y ∈ set<Конфета> & ¬ЛЮБИТЬ(x, у)" }
                })
            {
                using (var accumulatingAssert = new AccumulatingAssert())
                {
                    var originalFormula = GariParser.ParseSentence(formula.Key);
                    Assert.IsNotNull(originalFormula, $"GariParser failed to parse sentence '{formula.Key}'");
                    accumulatingAssert.AssertEqual(
                        formula.Value,
                        LogicFormula.Negate(originalFormula).ToString(),
                        $"Input: {formula.Key} ({originalFormula})");
                }
            }
        }

        [ClassInitialize]
        public static void SetupOnce(TestContext unused)
        {
            SetupOnceCore();
        }

        [ClassCleanup]
        public static void TeardownOnce()
        {
            TeardownOnceCore();
        }
    }
}
