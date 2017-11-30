using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Gari.Tests
{
    internal sealed class AccumulatingAssert : IDisposable
    {
        public void AssertEqual(string expectedValue, string actualValue, string extraInfo = null)
        {
            Assert.AreEqual(expectedValue, actualValue, extraInfo);

            if (expectedValue != actualValue)
            {
                _failedAssertions.Add($"Expected: {expectedValue}, Actual: {actualValue} {extraInfo}");
            }
            else
            {
                Trace.WriteLine($"Assert.AreEqual({expectedValue}, {actualValue}) succeeded. {extraInfo}");
            }
        }

        public void Dispose()
        {
            if (DisposeIsCalledBecauseTheUsingBlockHasExitedWithException())
            {
                if (_failedAssertions.Any())
                {
                    Trace.WriteLine($"The following Assertions failed during execution: {Environment.NewLine}{string.Join(Environment.NewLine, _failedAssertions)}");
                }

                return;
            }

            Assert.IsFalse(_failedAssertions.Any(), string.Join(Environment.NewLine, _failedAssertions));
        }

        private static bool DisposeIsCalledBecauseTheUsingBlockHasExitedWithException()
        {
            return Marshal.GetExceptionPointers() != IntPtr.Zero || Marshal.GetExceptionCode() != 0;
        }

        private readonly List<string> _failedAssertions = new List<string>();
    }
}