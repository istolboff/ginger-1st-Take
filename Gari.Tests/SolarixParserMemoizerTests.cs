using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ParseOzhegovWithSolarix.Solarix;

namespace Gari.Tests
{
    [TestClass]
    public class SolarixParserMemoizerTests
    {
        [TestMethod]
        public void TestSerializingOfSentenceElementSequence()
        {
            var original = new SentenceElement(
                "content", 
                new[] 
                {
                    new LemmaVersion("foo", PartOfSpeech.Глагол, new VerbCharacteristics(Case.Винительный, Number.Единственное, VerbForm.Повелительное, Person.Первое, VerbAspect.Несовершенный, Tense.Прошедшее)),
                    new LemmaVersion("bar", null, new AdjectiveCharacteristics(Case.Prepositive, Number.Множественное, Gender.Женский, AdjectiveForm.Краткое, ComparisonForm.Превосходная))
                }, 
                Enumerable.Empty<SentenceElement>(),
                LinkType.ACTOR_link);

            var serializedData = JsonConvert.SerializeObject(
                new[] { original, original },
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            var deserialized = JsonConvert.DeserializeObject<SentenceElement[]>(
                serializedData,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            Assert.IsNotNull(deserialized[0].LemmaVersions.First().Characteristics);
        }
    }
}
