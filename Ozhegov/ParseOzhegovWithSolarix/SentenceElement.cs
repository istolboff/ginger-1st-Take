using ParseOzhegovWithSolarix.Miscellaneous;
using System.Collections.Generic;

namespace ParseOzhegovWithSolarix
{
    public sealed class SentenceElement
    {
        public SentenceElement(string value, int position, IEnumerable<LemmaVersion> lemmaVersions, IEnumerable<SentenceElement> children, LinkType? leafType)
        {
            Value = value;
            Position = position;
            LeafType = leafType;
            LemmaVersions = lemmaVersions.AsImmutable();
            Children = children.AsImmutable();
        }

        public string Value { get; }

        public int Position { get; }

        public LinkType? LeafType { get; }

        public IEnumerable<LemmaVersion> LemmaVersions { get; }

        public IReadOnlyCollection<SentenceElement> Children { get; }
    }
}
