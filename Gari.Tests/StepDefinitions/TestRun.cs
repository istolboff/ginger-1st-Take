using System;
using System.Runtime.CompilerServices;
using TechTalk.SpecFlow;

namespace Gari.Tests.StepDefinitions
{
    [Binding]
    public class TestRun : NaturalLanguageSentenceParsingTestBase
    {
        [BeforeTestRun]
        public static void SetupTestrun()
        {
            SetupOnceCore();
        }

        [AfterTestRun]
        public static void TeardownTestrun()
        {
            TeardownOnceCore();
        }

        [BeforeScenario]
        public static void SetupScenario()
        {
        }

        [AfterScenario]
        public static void TeardownScenario()
        {
        }

        internal static T EstablishLazyReadOnlyProperty<T>(Func<T> createPropertyValue, [CallerMemberName] string propertyName = null)
        {
            if (ScenarioContext.Current.TryGetValue(propertyName, out T result))
            {
                return result;
            }

            result = createPropertyValue();
            ScenarioContext.Current.Set(result, propertyName);
            return result;
        }
    }
}
