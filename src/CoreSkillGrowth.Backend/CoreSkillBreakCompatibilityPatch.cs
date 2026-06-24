using System;
using System.Reflection;
using CoreSkillGrowth.Shared;
using GameData.Common;
using GameData.Domains.CombatSkill;
using GameData.Domains.Taiwu;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

internal static class CoreSkillBreakCompatibility
{
    private const sbyte CompatibilityGrade = 0;
    private const int FallbackBreakCostExp = 10;
    private const int FallbackPracticeQualificationRequirement = 30;
    private const sbyte FallbackBaseAvailableSteps = 20;
    private const byte FallbackBaseSuccessRate = 75;

    internal static bool IsCoreSkill(short skillId)
    {
        return skillId == CoreSkillGrowthConfigPatch.PeiRanJue ||
            skillId == CoreSkillGrowthConfigPatch.XiaoZongYueGong ||
            skillId == CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong ||
            skillId == CoreSkillGrowthConfigPatch.TaiZuChangQuan;
    }

    internal static int BreakCostExp()
    {
        try
        {
            return Config.SkillBreakPlate.Instance[CompatibilityGrade].CostExp;
        }
        catch
        {
            return FallbackBreakCostExp;
        }
    }

    internal static int PracticeQualificationRequirement()
    {
        try
        {
            return Config.SkillGradeData.Instance[CompatibilityGrade].PracticeQualificationRequirement;
        }
        catch
        {
            return FallbackPracticeQualificationRequirement;
        }
    }

    internal static sbyte BaseAvailableSteps()
    {
        try
        {
            return GlobalConfig.Instance.BreakoutBaseAvailableStepsCount;
        }
        catch
        {
            return FallbackBaseAvailableSteps;
        }
    }

    internal static byte BaseSuccessRate()
    {
        try
        {
            return (byte)Math.Clamp(30 + (9 - CompatibilityGrade) * 5, 0, 100);
        }
        catch
        {
            return FallbackBaseSuccessRate;
        }
    }

    internal static sbyte Max(sbyte current, sbyte minimum)
    {
        return current >= minimum ? current : minimum;
    }

    internal static byte Max(byte current, byte minimum)
    {
        return current >= minimum ? current : minimum;
    }

    internal static bool Prepare(MethodBase target, string patchName)
    {
        if (target != null)
        {
            return true;
        }

        Console.WriteLine($"[XuanShuFourArts] {patchName} skipped: target method not found.");
        return false;
    }
}

[HarmonyPatch]
internal static class CoreSkillBreakCostPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(TaiwuDomain), "GetBreakBaseCostExp", new[] { typeof(short) });
    }

    [HarmonyPrepare]
    private static bool Prepare()
    {
        return CoreSkillBreakCompatibility.Prepare(TargetMethod(), nameof(CoreSkillBreakCostPatch));
    }

    [HarmonyPostfix]
    private static void UseLowGradeBreakCost(short skillId, ref int __result)
    {
        try
        {
            if (!CoreSkillBreakCompatibility.IsCoreSkill(skillId))
            {
                return;
            }

            __result = Math.Min(__result, CoreSkillBreakCompatibility.BreakCostExp());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] core skill break cost compatibility failed: {ex}");
        }
    }
}

[HarmonyPatch]
internal static class CoreSkillBreakAvailableStepsPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(GameData.Domains.Character.Character),
            "GetSkillBreakoutAvailableStepsCount",
            new[]
            {
                typeof(short),
                typeof(CombatSkillBreakAvailableStepsDisplayData),
                typeof(bool)
            });
    }

    [HarmonyPrepare]
    private static bool Prepare()
    {
        return CoreSkillBreakCompatibility.Prepare(TargetMethod(), nameof(CoreSkillBreakAvailableStepsPatch));
    }

    [HarmonyPostfix]
    private static void UseLowGradeBreakSteps(
        short combatSkillTemplateId,
        CombatSkillBreakAvailableStepsDisplayData displayData,
        ref sbyte __result)
    {
        try
        {
            if (!CoreSkillBreakCompatibility.IsCoreSkill(combatSkillTemplateId))
            {
                return;
            }

            sbyte baseSteps = CoreSkillBreakCompatibility.BaseAvailableSteps();
            __result = CoreSkillBreakCompatibility.Max(__result, baseSteps);

            if (displayData != null)
            {
                displayData.BaseBaseAvailableSteps =
                    CoreSkillBreakCompatibility.Max(displayData.BaseBaseAvailableSteps, baseSteps);
                displayData.BaseAvailableSteps =
                    CoreSkillBreakCompatibility.Max(displayData.BaseAvailableSteps, __result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] core skill break steps compatibility failed: {ex}");
        }
    }
}

[HarmonyPatch]
internal static class CoreSkillBreakSuccessRatePatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(TaiwuDomain),
            "CalcTaiwuBreakBaseSuccessRate",
            new[]
            {
                typeof(Config.CombatSkillItem),
                typeof(CombatSkillBreakSuccessRateDisplayData),
                typeof(bool)
            });
    }

    [HarmonyPrepare]
    private static bool Prepare()
    {
        return CoreSkillBreakCompatibility.Prepare(TargetMethod(), nameof(CoreSkillBreakSuccessRatePatch));
    }

    [HarmonyPostfix]
    private static void UseLowGradeBreakSuccessRate(
        Config.CombatSkillItem skillConfig,
        CombatSkillBreakSuccessRateDisplayData displayData,
        ref byte __result)
    {
        try
        {
            if (skillConfig == null || !CoreSkillBreakCompatibility.IsCoreSkill(skillConfig.TemplateId))
            {
                return;
            }

            byte baseSuccessRate = CoreSkillBreakCompatibility.BaseSuccessRate();
            __result = CoreSkillBreakCompatibility.Max(__result, baseSuccessRate);

            if (displayData != null)
            {
                displayData.BaseBaseSuccessRate =
                    CoreSkillBreakCompatibility.Max(displayData.BaseBaseSuccessRate, baseSuccessRate);
                displayData.BaseSuccessRate =
                    CoreSkillBreakCompatibility.Max(displayData.BaseSuccessRate, __result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] core skill break success compatibility failed: {ex}");
        }
    }
}

[HarmonyPatch]
internal static class CoreSkillBreakInfoPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(TaiwuDomain),
            "GetEnterSkillBreakPlateInfo",
            new[]
            {
                typeof(DataContext),
                typeof(short)
            });
    }

    [HarmonyPrepare]
    private static bool Prepare()
    {
        return CoreSkillBreakCompatibility.Prepare(TargetMethod(), nameof(CoreSkillBreakInfoPatch));
    }

    [HarmonyPostfix]
    private static void UseLowGradeBreakInfo(short skillId, CharacterMenuSkillBreakData __result)
    {
        try
        {
            if (__result == null || !CoreSkillBreakCompatibility.IsCoreSkill(skillId))
            {
                return;
            }

            __result.BaseCostExp = Math.Min(__result.BaseCostExp, CoreSkillBreakCompatibility.BreakCostExp());
            __result.RequireQualification = Math.Min(
                __result.RequireQualification,
                CoreSkillBreakCompatibility.PracticeQualificationRequirement());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] core skill break info compatibility failed: {ex}");
        }
    }
}

[HarmonyPatch]
internal static class CoreSkillEnterBreakPlatePatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(TaiwuDomain),
            "EnterSkillBreakPlate",
            new[]
            {
                typeof(DataContext),
                typeof(short),
                typeof(ushort)
            });
    }

    [HarmonyPrepare]
    private static bool Prepare()
    {
        return CoreSkillBreakCompatibility.Prepare(TargetMethod(), nameof(CoreSkillEnterBreakPlatePatch));
    }

    [HarmonyPostfix]
    private static void UseLowGradeBreakPlateRuntime(short skillId, GameData.Domains.Taiwu.SkillBreakPlate __result)
    {
        try
        {
            if (__result == null || !CoreSkillBreakCompatibility.IsCoreSkill(skillId))
            {
                return;
            }

            __result.StepBase = Math.Max(__result.StepBase, CoreSkillBreakCompatibility.BaseAvailableSteps());
            __result.BaseSuccessRate =
                CoreSkillBreakCompatibility.Max(__result.BaseSuccessRate, CoreSkillBreakCompatibility.BaseSuccessRate());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] core skill break plate runtime compatibility failed: {ex}");
        }
    }
}
