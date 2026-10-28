using System;
using System.Collections.Generic;
using CombatSkillDamageStepBonusDisplayData = GameData.Domains.Combat.CombatSkillDamageStepBonusDisplayData;
using CombatSkillItem = Config.CombatSkillItem;
using SkillBreakPageEffectImplementItem = Config.SkillBreakPageEffectImplementItem;
using CoreSkillGrowth.Shared;
using GameData.Combat.Math;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.CombatSkill;
using GameData.Domains.Taiwu;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

internal static class GrowthSkillValueRuntime
{
    internal static int FactorFor(CombatSkill skill)
    {
        return GrowthSettings.FactorForCharacter(skill.GetId().CharId);
    }

    internal static int Delta(int neutralValue, int factor)
    {
        return GrowthSettings.DeltaFromNeutral(neutralValue, factor);
    }

    internal static short ClampShort(int value)
    {
        return (short)Math.Clamp(value, 0, short.MaxValue);
    }

    internal static int ReplaceBase(int result, int neutralBase, int scaledBase, int otherBase)
    {
        int oldTotal = neutralBase + otherBase;
        if (oldTotal == 0)
        {
            return result;
        }

        long product = (long)result * (scaledBase + otherBase);
        long rounded = product >= 0
            ? (product + oldTotal / 2) / oldTotal
            : (product - oldTotal / 2) / oldTotal;
        return (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, rounded));
    }
}

[HarmonyPatch(typeof(CombatSkill), "GetPenetrations")]
internal static class GrowthTaiZuPenetratePatch
{
    [HarmonyPostfix]
    private static void ScaleTaiZuPenetrate(CombatSkill __instance, ref OuterAndInnerInts __result)
    {
        try
        {
            if (__instance.GetId().SkillTemplateId != CoreSkillGrowthConfigPatch.TaiZuChangQuan)
            {
                return;
            }

            int neutral = StrengthSettings.ScaleShort(420, StrengthSettings.BloodRift);
            int delta = GrowthSkillValueRuntime.Delta(neutral, GrowthSkillValueRuntime.FactorFor(__instance));
            CValuePercentBonus breakoutBonus = __instance.GetBreakoutGridCombatSkillPropertyBonus(29);
            CValuePercent power = __instance.GetPower();
            int totalDelta = delta * breakoutBonus * power;
            int innerDelta = totalDelta * __instance.GetCurrInnerRatio() / 100;
            __result.Inner += innerDelta;
            __result.Outer += totalDelta - innerDelta;
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth TaiZu penetrate failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatSkill), "GetTotalHit")]
internal static class GrowthTaiZuHitPatch
{
    [HarmonyPostfix]
    private static void ScaleTaiZuHit(CombatSkill __instance, ref short __result)
    {
        try
        {
            if (__instance.GetId().SkillTemplateId != CoreSkillGrowthConfigPatch.TaiZuChangQuan)
            {
                return;
            }

            int neutral = StrengthSettings.ScaleShort(140, StrengthSettings.BloodRift);
            int delta = GrowthSkillValueRuntime.Delta(neutral, GrowthSkillValueRuntime.FactorFor(__instance));
            CValuePercentBonus bonus = __instance.GetBreakoutGridCombatSkillPropertyBonus(30);
            foreach (SkillBreakPageEffectImplementItem pageEffect in __instance.GetPageEffects())
            {
                bonus += (CValuePercentBonus)pageEffect.HitFactor;
            }
            foreach (SkillBreakPlateBonus breakBonus in __instance.GetBreakBonuses())
            {
                bonus += (CValuePercentBonus)breakBonus.CalcTotalHit();
            }
            __result = GrowthSkillValueRuntime.ClampShort(__result + delta * bonus);
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth TaiZu hit failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatSkill), "CalcInjuryDamageStep")]
internal static class GrowthInjuryDamageStepPatch
{
    [HarmonyPostfix]
    private static void ScaleTemplateContribution(CombatSkill __instance, bool inner, sbyte bodyPart, ref int __result)
    {
        try
        {
            short skillId = __instance.GetId().SkillTemplateId;
            if (!GrowthDamageStepValues.TryGetNeutral(skillId, inner, bodyPart, out int neutral))
            {
                return;
            }

            int delta = GrowthSkillValueRuntime.Delta(neutral, GrowthSkillValueRuntime.FactorFor(__instance));
            CombatSkillDamageStepBonusDisplayData bonuses = __instance.CalcStepBonusDisplayData();
            __result += delta * (inner ? bonuses.InnerInjuryStepBonus : bonuses.OuterInjuryStepBonus);
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth injury damage step failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatSkill), "CalcFatalDamageStep")]
internal static class GrowthFatalDamageStepPatch
{
    [HarmonyPostfix]
    private static void ScaleTemplateContribution(CombatSkill __instance, ref int __result)
    {
        try
        {
            if (__instance.GetId().SkillTemplateId != CoreSkillGrowthConfigPatch.PeiRanJue)
            {
                return;
            }

            int neutral = StrengthSettings.Scale(300, StrengthSettings.ReturningFrame);
            int delta = GrowthSkillValueRuntime.Delta(neutral, GrowthSkillValueRuntime.FactorFor(__instance));
            __result += delta * __instance.CalcStepBonusDisplayData().FatalStepBonus;
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth fatal damage step failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatSkill), "CalcMindDamageStep")]
internal static class GrowthMindDamageStepPatch
{
    [HarmonyPostfix]
    private static void ScaleTemplateContribution(CombatSkill __instance, ref int __result)
    {
        try
        {
            if (__instance.GetId().SkillTemplateId != CoreSkillGrowthConfigPatch.PeiRanJue)
            {
                return;
            }

            int neutral = StrengthSettings.Scale(50, StrengthSettings.ReturningFrame);
            int delta = GrowthSkillValueRuntime.Delta(neutral, GrowthSkillValueRuntime.FactorFor(__instance));
            __result += delta * __instance.CalcStepBonusDisplayData().MindStepBonus;
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth mind damage step failed: {ex}");
        }
    }
}

internal static class GrowthDamageStepValues
{
    private static readonly int[] ShadowSteps = { 0, 0, 0, 0, 0, 100, 100 };
    private static readonly int[] FormSteps = { 200, 200, 0, 0, 0, 0, 0 };
    private static readonly int[] BloodRiftSteps = { 0, 0, 0, 100, 100, 0, 0 };

    internal static bool TryGetNeutral(short skillId, bool inner, sbyte bodyPart, out int neutral)
    {
        neutral = 0;
        if (bodyPart < 0 || bodyPart >= 7)
        {
            return false;
        }

        int slider;
        int[] values;
        if (skillId == CoreSkillGrowthConfigPatch.XiaoZongYueGong)
        {
            slider = StrengthSettings.ShadowStep;
            values = ShadowSteps;
        }
        else if (skillId == CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong)
        {
            slider = StrengthSettings.FormRestoring;
            values = FormSteps;
        }
        else if (skillId == CoreSkillGrowthConfigPatch.TaiZuChangQuan)
        {
            slider = StrengthSettings.BloodRift;
            values = BloodRiftSteps;
        }
        else
        {
            return false;
        }

        neutral = StrengthSettings.Scale(values[bodyPart], slider);
        return true;
    }
}

[HarmonyPatch(typeof(CombatSkill), "GetTotalObtainableNeili")]
internal static class GrowthTotalNeiliPatch
{
    [HarmonyPostfix]
    private static void ScaleTemplateContribution(CombatSkill __instance, ref short __result)
    {
        try
        {
            if (__instance.GetId().SkillTemplateId != CoreSkillGrowthConfigPatch.PeiRanJue)
            {
                return;
            }

            int neutral = StrengthSettings.ScaleShort(2000, StrengthSettings.ReturningFrame);
            int delta = GrowthSkillValueRuntime.Delta(neutral, GrowthSkillValueRuntime.FactorFor(__instance));
            __result = GrowthSkillValueRuntime.ClampShort(__result + delta);
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth total Neili failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatSkillDomain), "CalcNeigongLoopingEffect")]
internal static class GrowthNeigongLoopPatch
{
    [HarmonyPostfix]
    private static void ScaleTemplateContribution(
        Character character,
        CombatSkillItem skillCfg,
        ref (short neili, short qiDisorder, int[] extraNeiliAllocationProgress) __result)
    {
        try
        {
            if (character == null || skillCfg.TemplateId != CoreSkillGrowthConfigPatch.PeiRanJue)
            {
                return;
            }

            int factor = GrowthSettings.FactorForCharacter(character);
            int neutralNeili = StrengthSettings.ScaleShort(60, StrengthSettings.ReturningFrame);
            int scaledNeili = GrowthSettings.ScaleByFactor(neutralNeili, factor);
            CombatSkill skill = DomainManager.CombatSkill.GetElement_CombatSkills(
                new CombatSkillKey(character.GetId(), skillCfg.TemplateId));
            int otherNeili = skill.GetBreakoutGridCombatSkillPropertyBonus(7);
            __result.neili = GrowthSkillValueRuntime.ClampShort(GrowthSkillValueRuntime.ReplaceBase(
                __result.neili, neutralNeili, scaledNeili, otherNeili));

            int neutralExtra = StrengthSettings.ScaleSByte(12, StrengthSettings.ReturningFrame);
            int scaledExtra = GrowthSettings.ScaleByFactor(neutralExtra, factor);
            for (int i = 0; i < __result.extraNeiliAllocationProgress.Length; i++)
            {
                __result.extraNeiliAllocationProgress[i] = GrowthSkillValueRuntime.ReplaceBase(
                    __result.extraNeiliAllocationProgress[i], neutralExtra, scaledExtra, 0);
            }
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth Neigong loop failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(TaiwuDomain), "TryAddLoopingEvent")]
internal static class GrowthQiArtStrategyPatch
{
    [HarmonyPrefix]
    private static void ScaleTemplateContribution(ref int basePercentProb)
    {
        try
        {
            List<short> referenceSkills = DomainManager.Extra.GetReferenceSkillList();
            if (referenceSkills == null || !referenceSkills.Contains(CoreSkillGrowthConfigPatch.PeiRanJue))
            {
                return;
            }

            int factor = GrowthSettings.FactorForCharacter(DomainManager.Taiwu.GetTaiwuCharId());
            int neutral = StrengthSettings.ScaleSByte(8, StrengthSettings.ReturningFrame);
            basePercentProb += GrowthSkillValueRuntime.Delta(neutral, factor);
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth Qi Art strategy failed: {ex}");
        }
    }
}
