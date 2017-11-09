using System;
using System.Linq;
using ParseOzhegovWithSolarix.PredicateLogic;
using ParseOzhegovWithSolarix.Solarix;

namespace ParseOzhegovWithSolarix
{
    public sealed class RussianGariParser : IDisposable
    {
        public RussianGariParser()
        {
            _russianGrammarEngine.Initialize();
        }

        public object ParseSentence(string sentenceText)
        {
            var sentenceStructure = _russianGrammarEngine.Parse(sentenceText);
            foreach (var parsingVariant in sentenceStructure)
            {
                var firstRootLemma = parsingVariant.LemmaVersions.First();
                if (firstRootLemma.PartOfSpeech == PartOfSpeech.Прилагательное &&
                    ((AdjectiveCharacteristics)firstRootLemma.Characteristics).Number == Number.Единственное &&
                    ((AdjectiveCharacteristics)firstRootLemma.Characteristics).AdjectiveForm == AdjectiveForm.Краткое &&
                    ((AdjectiveCharacteristics)firstRootLemma.Characteristics).ComparisonForm == ComparisonForm.Атрибут)
                {
                    if (parsingVariant.Children.Count == 1)
                    {
                        var singleChild = parsingVariant.Children.Single();
                        var firstChildLemma = singleChild.LemmaVersions.First();
                        if (singleChild.LeafType == LinkType.SUBJECT_link &&
                            firstChildLemma.PartOfSpeech == PartOfSpeech.Существительное &&
                            ((NounCharacteristics)firstChildLemma.Characteristics).Case == Case.Именительный &&
                            ((NounCharacteristics)firstChildLemma.Characteristics).Number == Number.Единственное &&
                            ((NounCharacteristics)firstChildLemma.Characteristics).Gender == ((AdjectiveCharacteristics)firstRootLemma.Characteristics).Gender)
                        {
                            return new LogicPredicate(firstRootLemma.Lemma, new LogicVariable(firstChildLemma.Lemma));
                        }
                    }
                }
            }

            return null;
        }

        public void Dispose()
        {
            _russianGrammarEngine.Dispose();
        }

        private readonly SolarixRussianGrammarEngine _russianGrammarEngine = new SolarixRussianGrammarEngine();
    }
}
