using SolarixGrammarEngineNET;

namespace ParseOzhegovWithSolarix
{
    public enum PartOfSpeech
    {
        Существительное = GrammarEngineAPI.NOUN_ru,
        Прилагательное = GrammarEngineAPI.ADJ_ru,
        Наречие = GrammarEngineAPI.ADVERB_ru,
        Глагол = GrammarEngineAPI.VERB_ru,
        Местоимение = GrammarEngineAPI.PRONOUN_ru,
        Инфинитив = GrammarEngineAPI.INFINITIVE_ru,
        Предлог = GrammarEngineAPI.PREPOS_ru,
        Союз = GrammarEngineAPI.CONJ_ru,
        Деепричастие = GrammarEngineAPI.GERUND_2_ru,
        Пунктуатор = GrammarEngineAPI.PUNCTUATION_class,
        Частица = GrammarEngineAPI.PARTICLE_ru,
        Местоим_Сущ = GrammarEngineAPI.PRONOUN2_ru,
        Притяж_Частица = GrammarEngineAPI.POSESS_PARTICLE,
        Num_Word = GrammarEngineAPI.NUM_WORD_CLASS
    }
}