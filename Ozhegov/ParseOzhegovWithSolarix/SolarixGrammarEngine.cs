using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SolarixGrammarEngineNET;
using ParseOzhegovWithSolarix.Miscellaneous;
using System.Diagnostics;

namespace ParseOzhegovWithSolarix
{
    public sealed class SolarixGrammarEngine : IDisposable
    {
        public SolarixGrammarEngine()
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
            Trace.Write($"parsing {text}...  ");
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

                Trace.WriteLine($"ok.");

                return Enumerable.Range(1, GrammarEngine.sol_CountRoots(hPack, 0) - 2)
                    .Select(i => CreateSentenceElement(hPack, GrammarEngine.sol_GetRoot(hPack, 0, i)))
                    .AsImmutable();
            }
            finally
            {
                GrammarEngine.sol_DeleteResPack(hPack);
            }
        }

        private SentenceElement CreateSentenceElement(IntPtr hPack, IntPtr hNode, int? leafType = null)
        {
            var value = new StringBuilder(LongestWordLength);
            GrammarEngine.sol_GetNodeContents(hNode, value);

            var lemmaVersions = Enumerable.Range(0, GrammarEngine.sol_GetNodeVersionCount(_engineHandle, hNode))
                .Select(versionIndex => GrammarEngine.sol_GetNodeVerIEntry(_engineHandle, hNode, versionIndex))
                .Select(id_entry =>
                {
                    var lemma = new StringBuilder(LongestWordLength);
                    GrammarEngine.sol_GetEntryName(_engineHandle, id_entry, lemma);

                    var partOfSpeech = new StringBuilder();
                    if (id_entry != -1)
                    {
                        var id_pos = GrammarEngine.sol_GetEntryClass(_engineHandle, id_entry);
                        GrammarEngine.sol_GetClassName(_engineHandle, id_pos, partOfSpeech);
                    }

                    var partOfSpeechText = partOfSpeech.ToString();
                    return new LemmaVersion(
                        lemma.ToString(),
                        string.IsNullOrEmpty(partOfSpeechText) ? (PartOfSpeech?)null : (PartOfSpeech)Enum.Parse(typeof(PartOfSpeech), partOfSpeechText, true));
                });

            var children = Enumerable.Range(0, GrammarEngine.sol_CountLeafs(hNode))
                .Select(leaveIndex => CreateSentenceElement(
                    hPack, 
                    GrammarEngine.sol_GetLeaf(hNode, leaveIndex), 
                    GrammarEngine.sol_GetLeafLinkType(hNode, leaveIndex)));

            return new SentenceElement(
                position: GrammarEngine.sol_GetNodePosition(hNode), 
                value: value.ToString(),
                leafType: leafType == null || leafType.Value < 0 ? (LinkType?)null : (LinkType)leafType.Value,
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

        private static string DefaultSolarixDictionaryXmlPath
        {
            get
            {
                return Path.GetFullPath(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, @"dictionary.xml"));
            }
        }

        private IntPtr _engineHandle;

        private const int RussianLanguage = 2;
        private const int LongestWordLength = 100;
    }
}
