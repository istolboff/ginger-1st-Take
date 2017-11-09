using ParseOzhegovWithSolarix.Miscellaneous;
using System.Collections.Generic;

namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class SentenceElement
    {
        public SentenceElement(string content, int position, IEnumerable<LemmaVersion> lemmaVersions, IEnumerable<SentenceElement> children, LinkType? leafType)
        {
            Content = content;
            Position = position;
            LeafType = leafType;
            LemmaVersions = lemmaVersions.AsImmutable();
            Children = children.AsImmutable();

            System.Diagnostics.Trace.WriteLine(ToString());
        }

        public string Content { get; }

        public int Position { get; }

        public LinkType? LeafType { get; }

        public IReadOnlyCollection<LemmaVersion> LemmaVersions { get; }

        public IReadOnlyCollection<SentenceElement> Children { get; }

        public override string ToString()
        {
            return $"{Content} {LeafType} {string.Join(",", LemmaVersions)}";
        }
    }
}
