using System;
using System.Collections.Generic;
#if BACKEND
using GameData.Domains;
using GameData.Domains.Character;
#endif

namespace CoreSkillGrowth.Shared;

// Growth model: each core art's effective strength follows its wielder.
//   factor(char) = clamp(Base + JingChun*w1 + Reincarnations*w2 + AgeCurve
//                        + min(LearnedSkills, SkillCap)*w4, 1, Cap)
//   AgeCurve peaks at half the character's lifespan, then declines to zero.
//   effective% = slider% * factor / 100
// When disabled, factor is always 100 and the mod behaves like the original
// static-percentage release.
internal static class GrowthSettings
{
    internal const int DisabledFactor = 100;
    internal const int MinFactor = 1;

    internal static bool Enabled { get; private set; }
    internal static int BasePercent { get; private set; }
    internal static int JingChunPer { get; private set; }
    internal static int ReincPer { get; private set; }
    internal static int AgePerFiveYears { get; private set; }
    internal static int SkillPer { get; private set; }
    internal static int SkillCap { get; private set; }

    private static readonly string[] Keys =
    {
        "GrowthEnabled", "GrowthBasePercent", "GrowthJingChunPer", "GrowthReincPer",
        "GrowthAgePerFiveYears", "GrowthSkillPer", "GrowthSkillCap"
    };

    private static readonly int[] Defaults = { 1, 25, 6, 15, 2, 1, 60 };
    private static readonly int[] Mins = { 0, 1, 0, 0, 0, 0, 0 };
    private static readonly int[] Maxs = { 1, 100, 50, 50, 30, 10, 200 };

    internal static void Load(string modIdStr)
    {
        int[] values = new int[Keys.Length];
        Array.Copy(Defaults, values, values.Length);
        for (int i = 1; i < values.Length; i++)
        {
            int value = Defaults[i];
            try
            {
#if BACKEND
                bool found = DomainManager.Mod.GetSetting(modIdStr, Keys[i], ref value);
#else
                bool found = ModManager.GetSetting(modIdStr, Keys[i], ref value);
#endif
                if (!found && i == 4)
                {
                    int legacyValue = Defaults[i] * 2;
#if BACKEND
                    bool legacyFound = DomainManager.Mod.GetSetting(modIdStr, "GrowthAgePerDecade", ref legacyValue);
#else
                    bool legacyFound = ModManager.GetSetting(modIdStr, "GrowthAgePerDecade", ref legacyValue);
#endif
                    if (legacyFound)
                    {
                        value = (legacyValue + 1) / 2;
                        found = true;
                    }
                }
                if (!found) value = Defaults[i];
            }
            catch (Exception ex)
            {
                ModLog.Error($"[XuanShuFourArts] Growth setting {Keys[i]} unavailable, using default: {ex.Message}");
                value = Defaults[i];
            }
            values[i] = Math.Max(Mins[i], Math.Min(Maxs[i], value));
        }

        bool enabled = Defaults[0] != 0;
        try
        {
#if BACKEND
            bool found = DomainManager.Mod.GetSetting(modIdStr, Keys[0], ref enabled);
#else
            bool found = ModManager.GetSetting(modIdStr, Keys[0], ref enabled);
#endif
            if (!found) enabled = Defaults[0] != 0;
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] Growth setting {Keys[0]} unavailable, using default: {ex.Message}");
            enabled = Defaults[0] != 0;
        }
        Enabled = enabled;
        BasePercent = values[1];
        JingChunPer = values[2];
        ReincPer = values[3];
        AgePerFiveYears = values[4];
        SkillPer = values[5];
        SkillCap = values[6];
        ModLog.Info($"[XuanShuFourArts] Growth {(Enabled ? "enabled" : "disabled")}: base={BasePercent}% jc+{JingChunPer}/pt re+{ReincPer}/life age+{AgePerFiveYears}/5y skill+{SkillPer}/cap{SkillCap}");
    }

    internal static int ClampFactor(int factor)
    {
        // The configured slider strength is the ceiling: growth fills the gap
        // from BasePercent up to 100 and never exceeds the slider value.
        return Math.Max(MinFactor, Math.Min(DisabledFactor, factor));
    }

#if BACKEND
    private static readonly Dictionary<int, int> FactorCache = new Dictionary<int, int>();
    private static readonly Dictionary<int, int> FactorCacheTime = new Dictionary<int, int>();
    private const int FactorCacheTtlMs = 1000;
    private static readonly object FactorCacheLock = new object();

    [ThreadStatic]
    private static bool _computingFactor;

    // Growth factor for a concrete wielder; 100 when growth is disabled.
    // Short-TTL cache: these getters are hit from per-tick combat code.
    internal static int FactorForCharacter(Character character)
    {
        if (!Enabled || character == null || _computingFactor)
        {
            return DisabledFactor;
        }

        int charId = character.GetId();
        lock (FactorCacheLock)
        {
            int now = Environment.TickCount;
            if (FactorCache.TryGetValue(charId, out int cached) &&
                now - FactorCacheTime[charId] < FactorCacheTtlMs)
            {
                return cached;
            }
        }

        int factor;
        _computingFactor = true;
        try
        {
            factor = ComputeFactor(character);
        }
        finally
        {
            _computingFactor = false;
        }

        lock (FactorCacheLock)
        {
            FactorCache[charId] = factor;
            FactorCacheTime[charId] = Environment.TickCount;
        }
        return factor;
    }

    private static int ComputeFactor(Character character)
    {
        try
        {
            int factor = BasePercent;
            factor += character.GetConsummateLevel() * JingChunPer;
            factor += CalculateAgeBonus(character.GetCurrAge(), character.GetMaxHealth(), AgePerFiveYears);
            List<short> learned = character.GetLearnedCombatSkills();
            int learnedCount = learned?.Count ?? 0;
            factor += Math.Min(learnedCount, SkillCap) * SkillPer;
            if (ReincPer != 0 && IsTaiwu(character))
            {
                factor += DomainManager.Taiwu.GetTaiwuGenerationsCount() * ReincPer;
            }
            int clamped = ClampFactor(factor);
            if (ModLog.DebugEnabled)
            {
                ModLog.Debug($"growth char={character.GetId()} factor={clamped} raw={factor} age={character.GetCurrAge()} lifespanMonths={character.GetMaxHealth()} skills={learnedCount}");
            }
            return clamped;
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth factor failed for char {character.GetId()}: {ex}");
            return ClampFactor(BasePercent);
        }
    }

    internal static int CalculateAgeBonus(int age, int lifespanMonths, int weight)
    {
        if (age <= 0 || lifespanMonths <= 0 || weight <= 0)
        {
            return 0;
        }

        double lifespanRatio = Math.Clamp(age * 12.0 / lifespanMonths, 0.0, 1.0);
        double bonus = weight * (age / 5.0) * 2.0 * (1.0 - lifespanRatio);
        return (int)Math.Round(bonus, MidpointRounding.AwayFromZero);
    }

    internal static int FactorForCharacter(int charId)
    {
        if (!Enabled)
        {
            return DisabledFactor;
        }
        try
        {
            return FactorForCharacter(DomainManager.Character.GetElement_Objects(charId));
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth factor failed for charId {charId}: {ex}");
            return ClampFactor(BasePercent);
        }
    }

    private static bool IsTaiwu(Character character)
    {
        try
        {
            return DomainManager.Taiwu.GetTaiwuCharId() == character.GetId();
        }
        catch
        {
            return false;
        }
    }
#endif

    // value already carries slider%; multiply by factor% with rounding.
    internal static int ScaleByFactor(int value, int factor)
    {
        long product = (long)value * factor;
        long rounded = product >= 0 ? (product + 50) / 100 : (product - 50) / 100;
        return (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, rounded));
    }

    internal static short ScaleShortByFactor(int value, int factor)
    {
        return (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, ScaleByFactor(value, factor)));
    }

    // Delta between the neutral bake (factor 100) and the wielder's factor,
    // applied on top of values baked at slider strength.
    internal static int DeltaFromNeutral(int bakedValue, int factor)
    {
        return ScaleByFactor(bakedValue, factor - DisabledFactor);
    }
}
