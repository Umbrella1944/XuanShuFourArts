using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreSkillGrowth.Shared;

internal static class ModLanguage
{
    private const uint LcMapTraditionalChinese = 0x04000000;

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

    internal static string ToTraditionalChinese(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        try
        {
            int length = LCMapStringEx("zh-CN", LcMapTraditionalChinese, value, -1, null, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (length <= 0)
            {
                return value;
            }

            StringBuilder builder = new StringBuilder(length);
            int written = LCMapStringEx("zh-CN", LcMapTraditionalChinese, value, -1, builder, length, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            return written > 0 ? builder.ToString().TrimEnd('\0') : value;
        }
        catch
        {
            return value;
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int LCMapStringEx(
        string localeName,
        uint flags,
        string source,
        int sourceLength,
        StringBuilder destination,
        int destinationLength,
        IntPtr versionInformation,
        IntPtr reserved,
        IntPtr sortHandle);

    internal static string SelectCurrent(string chinese, string english, string korean, string japanese, string traditionalChinese = null)
    {
        switch (Current)
        {
            case "CN":
                return chinese;
            case "CNH":
                return traditionalChinese ?? ToTraditionalChinese(chinese);
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
                return traditionalChinese ?? ToTraditionalChinese(chinese);
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
                return traditionalChinese ?? ToTraditionalChinese(chinese);
            case "KO":
                return korean;
            case "EN":
                return english;
            default:
                return chinese;
        }
    }

    private static string[] ToTraditionalChinese(string[] values)
    {
        if (values == null)
        {
            return null;
        }

        string[] result = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = ToTraditionalChinese(values[i]);
        }
        return result;
    }
}
