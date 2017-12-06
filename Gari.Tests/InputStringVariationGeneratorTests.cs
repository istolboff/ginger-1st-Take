using Gari.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Gari.Tests
{
    [TestClass]
    public class InputStringVariationGeneratorTests
    {
        [TestMethod]
        public void TestNoOptionalAndAlternativeElements()
        {
            CollectionAssert.AreEqual(
                new[] { string.Empty },
                InputStringVariationGenerator.GenerateVariations(string.Empty).ToArray());

            CollectionAssert.AreEqual(
                new[] { "Сократ это человек" },
                InputStringVariationGenerator.GenerateVariations("Сократ это человек").ToArray());
        }

        [TestMethod]
        public void TestSingleOptionalElement()
        {
            CollectionAssert.AreEqual(
                new[] { "Сократ человек", "Сократ это человек" },
                InputStringVariationGenerator.GenerateVariations("Сократ [это] человек").ToArray());

            CollectionAssert.AreEqual(
                new[] { "человек", "это человек" },
                InputStringVariationGenerator.GenerateVariations("[это] человек").ToArray());

            CollectionAssert.AreEqual(
                new[] { "Сократ", "Сократ это" },
                InputStringVariationGenerator.GenerateVariations("Сократ [это]").ToArray());
        }

        [TestMethod]
        public void TestAdjacentOptionalElements()
        {
            CollectionAssert.AreEqual(
                new[] { "Сократ человек", "Сократ - человек", "Сократ это человек", "Сократ - это человек" },
                InputStringVariationGenerator.GenerateVariations("Сократ [-] [это] человек").ToArray());
        }

        [TestMethod]
        public void TestSingleElementsAlternative()
        {
            CollectionAssert.AreEqual(
                new[] { "Сократ это человек", "Сократ есть человек" },
                InputStringVariationGenerator.GenerateVariations("Сократ (это|есть) человек").ToArray());

            CollectionAssert.AreEqual(
                new[] { "это Сократ человек", "есть Сократ человек" },
                InputStringVariationGenerator.GenerateVariations("(это|есть) Сократ человек").ToArray());

            CollectionAssert.AreEqual(
                new[] { "Сократ человек это", "Сократ человек есть" },
                InputStringVariationGenerator.GenerateVariations("Сократ человек (это|есть)").ToArray());
        }

        [TestMethod]
        public void TestMultipleElementsAlternatives()
        {
            CollectionAssert.AreEqual(
                new[] { "Сократ это человек и птица", "Сократ есть человек и птица", "Сократ это человек или птица", "Сократ есть человек или птица" },
                InputStringVariationGenerator.GenerateVariations("Сократ (это|есть) человек (и|или) птица").ToArray());
        }

        [TestMethod]
        public void TestAlternativesCombinedWithOptionalElements()
        {
            CollectionAssert.AreEqual(
                new[] { "Сократ - человек", "Сократ это человек", "Сократ - это человек" },
                InputStringVariationGenerator.GenerateVariations("Сократ (-|[-] это) человек").ToArray());

            CollectionAssert.AreEqual(
                new[] { "Сократ - человек", "Сократ - человек est", "Сократ это человек", "Сократ это человек est" },
                InputStringVariationGenerator.GenerateVariations("Сократ (-|это) человек [est]").ToArray());
        }
    }
}
