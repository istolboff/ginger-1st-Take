using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix.Solarix
{
    public sealed class SentenceElement
    {
        public SentenceElement(string content, IEnumerable<LemmaVersion> lemmaVersions, IEnumerable<SentenceElement> children, LinkType? leafLinkType)
        {
            Content = content;
            LeafLinkType = leafLinkType;
            LemmaVersions = lemmaVersions.AsImmutable();
            Children = new ReadOnlyCollection<SentenceElement>(children.ToList());
        }

        public string Content { get; }

        public LinkType? LeafLinkType { get; }

        public IReadOnlyCollection<LemmaVersion> LemmaVersions { get; }

        public ReadOnlyCollection<SentenceElement> Children { get; }

        public override string ToString()
        {
            return $"{Content} {LeafLinkType} {string.Join(",", LemmaVersions)}";
        }
    }
}
