using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class SentenceElement
    {
        public SentenceElement(string content, int position, IEnumerable<LemmaVersion> lemmaVersions, IEnumerable<SentenceElement> children, LinkType? leafType)
        {
            Content = content;
            Position = position;
            LeafLinkType = leafType;
            LemmaVersions = lemmaVersions.AsImmutable();
            Children = new ReadOnlyCollection<SentenceElement>(children.ToList());

            System.Diagnostics.Trace.WriteLine(ToString());
        }

        public string Content { get; }

        public int Position { get; }

        public LinkType? LeafLinkType { get; }

        public IReadOnlyCollection<LemmaVersion> LemmaVersions { get; }

        public ReadOnlyCollection<SentenceElement> Children { get; }

        public override string ToString()
        {
            return $"{Content} {LeafLinkType} {string.Join(",", LemmaVersions)}";
        }
    }
}
