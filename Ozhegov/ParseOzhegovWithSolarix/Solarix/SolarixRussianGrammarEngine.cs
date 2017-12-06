using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SolarixGrammarEngineNET;
using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class SolarixRussianGrammarEngine : IRussianGrammarParser
    {
        public SolarixRussianGrammarEngine()
        {
            _engineHandle = GrammarEngine.sol_CreateGrammarEngineW(null);
            if (_engineHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not create Grammar Engine");
            }
        }

        public void Initialize(string dictionaryPath = null)
        {
            dictionaryPath = dictionaryPath ?? DefaultSolarixDictionaryXmlPath;

            var loadStatus = GrammarEngine.sol_LoadDictionaryExW(
                    _engineHandle,
                    dictionaryPath,
                    GrammarEngine.EngineInstanceFlags.SOL_GREN_LAZY_LEXICON);

            if (loadStatus != 1)
            {
                throw new InvalidOperationException($"Could not load Dictionary from {dictionaryPath}. {DescribeError()}");
            }
        }

        public IReadOnlyCollection<SentenceElement> Parse(string text)
        {
            IntPtr hPack = IntPtr.Zero;
            try
            {
                hPack = GrammarEngine.sol_SyntaxAnalysis(
                    _engineHandle,
                    text,
                    GrammarEngine.MorphologyFlags.SOL_GREN_MODEL,
                    GrammarEngine.SyntaxFlags.DEFAULT,
                    (20 << 22) | 30000,
                    RussianLanguage);

                if (hPack == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Could parse the text: {text}. {DescribeError()}");
                }

                var ngrafs = GrammarEngine.sol_CountGrafs(hPack);
                if (ngrafs <= 0)
                {
                    throw new InvalidOperationException($"No graphs were parsed from the text: {text}.");
                }

                return Enumerable.Range(1, GrammarEngine.sol_CountRoots(hPack, 0) - 2)
                    .Select(i => CreateSentenceElement(GrammarEngine.sol_GetRoot(hPack, 0, i)))
                    .AsImmutable();
            }
            finally
            {
                GrammarEngine.sol_DeleteResPack(hPack);
            }
        }

        private SentenceElement CreateSentenceElement(IntPtr hNode, int? leafType = null)
        {
            var content = new StringBuilder(LongestWordLength);
            GrammarEngine.sol_GetNodeContents(hNode, content);

            var lemmaVersions = Enumerable.Range(0, GrammarEngine.sol_GetNodeVersionCount(_engineHandle, hNode))
                .Select(versionIndex => 
                new
                {
                    VersionIndex = versionIndex,
                    EntryVersionId = GrammarEngine.sol_GetNodeVerIEntry(_engineHandle, hNode, versionIndex)
                })
                .Select(item =>
                {
                    var entryVersionId = item.EntryVersionId;
                    var versionIndex = item.VersionIndex;
                    var lemma = new StringBuilder(LongestWordLength);
                    GrammarEngine.sol_GetEntryName(_engineHandle, entryVersionId, lemma);

                    var partOfSpeechIndex = GrammarEngine.sol_GetEntryClass(_engineHandle, entryVersionId);
                    var partOfSpeech = partOfSpeechIndex < 0 ? (PartOfSpeech?)null : (PartOfSpeech)partOfSpeechIndex;
                    var grammarCharacteristcs = BuldGrammarCharacteristics(hNode, versionIndex, partOfSpeech);
                    foreach (var coordinateId in new[] { GrammarEngineAPI.CASE_ru, GrammarEngineAPI.NUMBER_ru, GrammarEngineAPI.GENDER_ru,
                                                 GrammarEngineAPI.VERB_FORM_ru, GrammarEngineAPI.PERSON_ru, GrammarEngineAPI.ASPECT_ru,
                                                 GrammarEngineAPI.TENSE_ru, GrammarEngineAPI.SHORTNESS_ru,
                                                 GrammarEngineAPI.FORM_ru, GrammarEngineAPI.COMPAR_FORM_ru })
                    {
                        var stateName = new StringBuilder(100);
                        GrammarEngine.sol_GetCoordName(_engineHandle, coordinateId, stateName);
                        var stateValue = new StringBuilder(100);
                        if (GrammarEngine.sol_CountCoordStates(_engineHandle, coordinateId) != 0)
                        {
                            var coordState = GrammarEngine.sol_GetNodeVerCoordState(hNode, versionIndex, coordinateId);

                            if (coordState < 0)
                            {
                                continue;
                            }

                            GrammarEngine.sol_GetCoordStateName(_engineHandle, coordinateId, coordState, stateValue);
                        }
                    }

                    return new LemmaVersion(lemma.ToString(), partOfSpeech, grammarCharacteristcs);
                });

            var children = Enumerable.Range(0, GrammarEngine.sol_CountLeafs(hNode))
                .Select(leaveIndex => CreateSentenceElement(
                    GrammarEngine.sol_GetLeaf(hNode, leaveIndex), 
                    GrammarEngine.sol_GetLeafLinkType(hNode, leaveIndex)));

            return new SentenceElement(
                content: content.ToString(),
                leafLinkType: leafType == null || leafType.Value < 0 ? (LinkType?)null : (LinkType)leafType.Value,
                lemmaVersions: lemmaVersions, 
                children: children);
        }

        public void Dispose()
        {
            if (_engineHandle != IntPtr.Zero)
            {
                GrammarEngine.sol_DeleteGrammarEngine(_engineHandle);
                _engineHandle = IntPtr.Zero;
            }
        }

        private string DescribeError()
        {
            var errorLength = GrammarEngine.sol_GetErrorLen(_engineHandle);
            var errorBuffer = new StringBuilder(errorLength);
            var errorCode = GrammarEngine.sol_GetError(_engineHandle, errorBuffer, errorLength);
            return errorCode == 1 ? errorBuffer.ToString() : "Unknown error";
        }

        private static string DefaultSolarixDictionaryXmlPath => 
            Path.GetFullPath(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, @"dictionary.xml"));

        private static GrammarCharacteristics BuldGrammarCharacteristics(IntPtr hNode, int versionIndex, PartOfSpeech? partOfSpeech) => 
            partOfSpeech == null || !GrammarCharacteristicsBuilders.TryGetValue(partOfSpeech.Value, out var builder)
                    ? new NullGrammarCharacteristics()
                    : builder(hNode, versionIndex);

        private static TState GetNodeVersionCoordinateState<TState>(IntPtr hNode, int versionIndex) where TState: struct 
        {
            var coordinateId = CoordinateStateTypeToCoordinateIdMap[typeof(TState)];
            var coordinateState = GrammarEngine.sol_GetNodeVerCoordState(hNode, versionIndex, coordinateId);

            if (coordinateState < 0)
            {
                throw new InvalidOperationException($"Could not get {typeof(TState).Name} coordinate");
            }

            return (TState)(object)coordinateState;
        }

        private static TState? TryGetNodeVersionCoordinateState<TState>(IntPtr hNode, int versionIndex) where TState : struct
        {
            var coordinateId = CoordinateStateTypeToCoordinateIdMap[typeof(TState)];
            var coordinateState = GrammarEngine.sol_GetNodeVerCoordState(hNode, versionIndex, coordinateId);
            return (coordinateState < 0) ? (TState?)null : (TState)(object)coordinateState;
        }

        private IntPtr _engineHandle;

        private static readonly IDictionary<PartOfSpeech, Func<IntPtr, int, GrammarCharacteristics>> GrammarCharacteristicsBuilders = 
            new Dictionary<PartOfSpeech, Func<IntPtr, int, GrammarCharacteristics>>
            {
                {
                    PartOfSpeech.Существительное,
                    (hNode, versionIndex) =>
                        new NounCharacteristics(
                            GetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<Number>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<Gender>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<Form>(hNode, versionIndex))
                },
                {
                    PartOfSpeech.Глагол,
                    (hNode, versionIndex) =>
                        new VerbCharacteristics(
                            TryGetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<Number>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<VerbForm>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<Person>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<VerbAspect>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<Tense>(hNode, versionIndex))
                },
                {
                    PartOfSpeech.Прилагательное,
                    (hNode, versionIndex) => 
                        new AdjectiveCharacteristics(
                            GetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<Number>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<Gender>(hNode, versionIndex),
                            GrammarEngine.sol_GetNodeVerCoordState(hNode, versionIndex, GrammarEngineAPI.SHORTNESS_ru) >= 0 ? AdjectiveForm.Краткое : AdjectiveForm.Полное,
                            GetNodeVersionCoordinateState<ComparisonForm>(hNode, versionIndex))
                },
                {
                    PartOfSpeech.Наречие,
                    (hNode, versionIndex) =>
                        new AdverbCharacteristics(GetNodeVersionCoordinateState<ComparisonForm>(hNode, versionIndex))
                },
                {
                    PartOfSpeech.Деепричастие,
                    (hNode, versionIndex) =>
                        new GerundCharacteristics(
                            GetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                            GetNodeVersionCoordinateState<VerbAspect>(hNode, versionIndex))
                }
            };

        private static readonly IDictionary<Type, int> CoordinateStateTypeToCoordinateIdMap = 
            new Dictionary<Type, int>
            {
                { typeof(Case), GrammarEngineAPI.CASE_ru },
                { typeof(Number), GrammarEngineAPI.NUMBER_ru },
                { typeof(Gender), GrammarEngineAPI.GENDER_ru },
                { typeof(Form), GrammarEngineAPI.FORM_ru },
                { typeof(Person), GrammarEngineAPI.PERSON_ru },
                { typeof(VerbForm), GrammarEngineAPI.VERB_FORM_ru },
                { typeof(VerbAspect), GrammarEngineAPI.ASPECT_ru },
                { typeof(Tense), GrammarEngineAPI.TENSE_ru },
                { typeof(ComparisonForm), GrammarEngineAPI.COMPAR_FORM_ru }
            };

        private const int RussianLanguage = 2;
        private const int LongestWordLength = 100;
    }
}
