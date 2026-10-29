using System;

namespace CoreSkillGrowth.Shared;

internal static class ModLanguage
{
    internal static string Current
    {
        get
        {
            try
            {
                return Normalize(LocalStringManager.CurLanguageKey);
            }
            catch
            {
                return "CN";
            }
        }
    }

    internal static string Normalize(string languageKey)
    {
        string key = (languageKey ?? string.Empty).Trim().ToUpperInvariant();
        return key == "CN" || key == "CNH" || key == "KO" || key == "EN" ? key : "CN";
    }

    internal static string SelectCurrent(string chinese, string english, string korean, string japanese, string traditionalChinese = null)
    {
        switch (Current)
        {
            case "CN":
                return chinese;
            case "CNH":
                return traditionalChinese ?? chinese;
            case "KO":
                return korean;
            case "EN":
                return english;
            default:
                return chinese;
        }
    }

    internal static string Select(string languageKey, string chinese, string english, string korean, string japanese, string traditionalChinese = null)
    {
        switch (Normalize(languageKey))
        {
            case "CN":
                return chinese;
            case "CNH":
                return traditionalChinese ?? chinese;
            case "KO":
                return korean;
            case "EN":
                return english;
            default:
                return chinese;
        }
    }

    internal static string[] Select(string languageKey, string[] chinese, string[] english, string[] korean, string[] japanese, string[] traditionalChinese = null)
    {
        switch (Normalize(languageKey))
        {
            case "CN":
                return chinese;
            case "CNH":
                return traditionalChinese ?? chinese;
            case "KO":
                return korean;
            case "EN":
                return english;
            default:
                return chinese;
        }
    }
}
