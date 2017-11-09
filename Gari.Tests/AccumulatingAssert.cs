using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gari.Tests
{
    internal sealed class AccumulatingAssert : IDisposable
    {
        public void AssertEqual(string expectedValue, string actualValue)
        {
            if (expectedValue != actualValue)
            {
                _failedAssertions.Add($"Expected: {expectedValue}, Actual: {actualValue}");
            }
        }

        public void Dispose()
        {
            Assert.IsFalse(_failedAssertions.Any(), string.Join(Environment.NewLine, _failedAssertions));
        }

        private readonly List<string> _failedAssertions = new List<string>();
    }
}