using System;
using System.Collections.Generic;
#if BACKEND
using GameData.Domains;
#endif

namespace CoreSkillGrowth.Shared;

internal static class ModLog
{
    private static readonly Dictionary<string, DateTime> LastWrites = new Dictionary<string, DateTime>();
    private static readonly object WriteLock = new object();
    private static readonly TimeSpan RepeatInterval = TimeSpan.FromSeconds(60);

    internal static bool DebugEnabled { get; private set; }

    internal static void Load(string modIdStr)
    {
        bool enabled = false;
        try
        {
#if BACKEND
            DomainManager.Mod.GetSetting(modIdStr, "DebugLoggingEnabled", ref enabled);
#else
            ModManager.GetSetting(modIdStr, "DebugLoggingEnabled", ref enabled);
#endif
        }
        catch (Exception ex)
        {
            Warn($"[XuanShuFourArts] Debug logging setting unavailable: {ex.Message}");
        }
        DebugEnabled = enabled;
    }

    internal static void Info(string message)
    {
        Console.WriteLine(message);
    }

    internal static void Warn(string message)
    {
        WriteLimited("WARN", message);
    }

    internal static void Error(string message)
    {
        WriteLimited("ERROR", message);
    }

    internal static void Debug(string message)
    {
        if (DebugEnabled)
        {
            Console.WriteLine($"[XuanShuFourArts][DEBUG] {message}");
        }
    }

    private static void WriteLimited(string level, string message)
    {
        string key = level + ":" + MessageKey(message);
        DateTime now = DateTime.UtcNow;
        lock (WriteLock)
        {
            if (LastWrites.TryGetValue(key, out DateTime lastWrite) && now - lastWrite < RepeatInterval)
            {
                return;
            }
            LastWrites[key] = now;
        }
        Console.WriteLine($"[XuanShuFourArts][{level}] {message}");
    }

    private static string MessageKey(string message)
    {
        const string prefix = "[XuanShuFourArts] ";
        string body = message != null && message.StartsWith(prefix, StringComparison.Ordinal)
            ? message.Substring(prefix.Length)
            : message ?? string.Empty;
        int separator = body.IndexOf(':');
        return separator >= 0 ? body.Substring(0, separator) : body;
    }
}
