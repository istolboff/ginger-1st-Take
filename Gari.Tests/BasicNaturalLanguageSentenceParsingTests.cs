using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gari.Tests
{
    [TestClass]
    public sealed class BasicNaturalLanguageSentenceParsingTests
    {
        [TestMethod]
        public void ElementaryNaturalLanguageSentencesShouldBeCorrectlyParsedToLogicalExpressions()
        {
            Assert.AreEqual(
                Predicate("папа").WithArguments("Петя", "Маша"),
                Parse(Sentence("Петя папа Маши")));
        }

        private LogicPredicate Predicate(string name)
        {
            throw new NotImplementedException();
        }

        private ParsableSentence Sentence(string v)
        {
            throw new NotImplementedException();
        }

        private object Parse(ParsableSentence parsableSentence)
        {
            throw new NotImplementedException();
        }

        private class LogicPredicate
        {
            public object WithArguments(string v1, string v2)
            {
                throw new NotImplementedException();
            }
        }

        private class ParsableSentence
        {
        }
    }
}
