using ParseOzhegovWithSolarix.Miscellaneous;
using ParseOzhegovWithSolarix.Solarix;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ParseOzhegovWithSolarix
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var grammarEngine = new SolarixRussianGrammarEngine())
            {
                grammarEngine.Initialize();

                var differentThings = 
                    File.ReadLines(@"C:\devl\Samples\tmp\Ozhegov\Ozhegov.txt", Encoding.GetEncoding("windows-1251"))
                    .Where(line => !string.IsNullOrEmpty(line))
                    .Select(line =>
                    {
                        foreach (var pattern in new Dictionary<string, string>
                                   {
                                        { "чьи-н.", "чьи-нибудь" },
                                        { "чему-н.", "чему-нибудь" },
                                        { "чем-н.", "чем-нибудь" },
                                        { "какое-н.", "какое-нибудь" },
                                        { " ед. ", " единственое " },
                                        { "мн. ", " множественное " }
                                   })
                        {
                            line = line.Replace(pattern.Key, pattern.Value);
                        }

                        return line;
                    })
                    .Select(line =>
                    {
                        var linePrefix = line.Substring(0, Math.Min(line.Length, 150));

                        var definedWordEnd = line.IndexOf(',');
                        if (definedWordEnd < 0)
                        {
                            return new { Line = linePrefix, DefinedWord = "????", Meaning = line, ParsingResult = (IReadOnlyCollection<SentenceElement>)new SentenceElement[0] };
                        }

                        var definedWord = line.Substring(0, definedWordEnd);
                        if (definedWord.EndsWith("]"))
                        {
                            var definedWordEndIndex = definedWord.LastIndexOf('[');
                            if (definedWordEndIndex > 0)
                            {
                                definedWord = definedWord.Substring(0, definedWordEndIndex);
                            }
                        }

                        definedWord = definedWord.TrimEnd(new[] { '.', '2', '3', '4', ' ' });

                        var firstDotOffset = line.IndexOf('.', definedWordEnd);
                        if (firstDotOffset < 0)
                        {
                            return new
                            {
                                Line = linePrefix,
                                DefinedWord = definedWord,
                                Meaning = line,
                                ParsingResult = (IReadOnlyCollection<SentenceElement>)new SentenceElement[0]
                            };
                        }

                        int secondDotOffset;
                        while (true)
                        {
                            secondDotOffset = line.IndexOf('.', firstDotOffset + 1);
                            if (secondDotOffset - firstDotOffset > 5)
                            {
                                break;
                            }

                            firstDotOffset = secondDotOffset;
                        }

                        var meaning = line.Substring(firstDotOffset + 1, secondDotOffset - firstDotOffset - 1);

                        return new
                        {
                            Line = linePrefix,
                            DefinedWord = definedWord,
                            Meaning = meaning,
                            ParsingResult = grammarEngine.Parse(meaning)
                        };
                    })
                    // .Skip(200)
                    .Take(5)
                    .GroupBy(item => item.ParsingResult.Count == 1 
                                    ? item.ParsingResult.Single() 
                                    : new SentenceElement(string.Empty, 0, new LemmaVersion[0], new SentenceElement[0], null))
                    .ToDictionary(i => i.Key, i => i.AsImmutable());

                Console.WriteLine(differentThings.ToString());
            }
        }

        private class ParsedStructureComparer : IEqualityComparer<SentenceElement>
        {
            public bool Equals(SentenceElement x, SentenceElement y)
            {
                if (x.LeafType != y.LeafType || x.Children.Count != y.Children.Count)
                {
                    return false;
                }

                if (x.LemmaVersions.All(xv => y.LemmaVersions.All(yv => xv.PartOfSpeech != yv.PartOfSpeech)))
                {
                    return false;
                }

                return x.Children.SequenceEqual(y.Children, this);
            }

            public int GetHashCode(SentenceElement obj)
            {
                return 1;
            }
        }
    }
}
