using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Gari.Tests
{
    [TestClass]
    public class InputStringVariationGeneratorTests
    {
        [TestMethod]
        public void TestNoOptionalElements()
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
    }
}
