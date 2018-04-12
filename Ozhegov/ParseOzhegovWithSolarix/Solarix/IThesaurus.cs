using ParseOzhegovWithSolarix.Miscellaneous;

namespace ParseOzhegovWithSolarix.Solarix
{
    interface IThesaurus
    {
        IOptional<string> FindLinkedParticipleInPassiveVoice(string word);
    }
}
