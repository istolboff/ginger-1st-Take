using SolarixGrammarEngineNET;

namespace ParseOzhegovWithSolarix.Solarix
{
    public enum Case
    {
        Именительный = GrammarEngineAPI.NOMINATIVE_CASE_ru,
        Звательный = GrammarEngineAPI.VOCATIVE_CASE_ru,
        Родительный  = GrammarEngineAPI.GENITIVE_CASE_ru,Prepositive,
        Партитив = GrammarEngineAPI.PARTITIVE_CASE_ru,
        Творительный = GrammarEngineAPI.INSTRUMENTAL_CASE_ru,
        Винительный = GrammarEngineAPI.ACCUSATIVE_CASE_ru,
        Дательный = GrammarEngineAPI.DATIVE_CASE_ru,
        Предложный = GrammarEngineAPI.PREPOSITIVE_CASE_ru,
        Местный = GrammarEngineAPI.LOCATIVE_CASE_ru,
    }
}
