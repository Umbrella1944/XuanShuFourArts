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
                return "EN";
            }
        }
    }

    internal static string Normalize(string languageKey)
    {
        string key = (languageKey ?? string.Empty).Trim().ToUpperInvariant();
        return key == "CN" || key == "CNH" || key == "KO" || key == "EN" ? key : "EN";
    }

    internal static string Select(string chinese, string english, string korean, string japanese, string traditionalChinese = null)
    {
        switch (Current)
        {
            case "CN":
                return chinese;
            case "CNH":
                return traditionalChinese ?? chinese;
            case "KO":
                return korean;
            default:
                return english;
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
            default:
                return english;
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
            default:
                return english;
        }
    }
}
