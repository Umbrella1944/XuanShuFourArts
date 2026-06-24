using System;
using System.Collections.Generic;
using System.Reflection;
using CoreSkillGrowth.Shared;
using GameData.Combat.Math;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

internal static class DecoupledCoreSkillEffects
{
    private const int ShuiHuoReduceInjuryCount = 1;

    internal static bool CanAffectSkill(CombatCharacter combatChar, short skillId)
    {
        return combatChar != null &&
            DomainManager.Combat.TryGetCombatSkillData(combatChar.GetId(), skillId, out CombatSkillData skillData) &&
            skillData.GetCanAffect();
    }

    internal static int ApplyXiaoZongDirectDamageConversion(CombatContext context, int damageValue)
    {
        CombatCharacter defender = context.Defender;
        if (defender == null ||
            damageValue <= 0 ||
            defender.GetAffectingMoveSkillId() != CoreSkillGrowthConfigPatch.XiaoZongYueGong ||
            !CanAffectSkill(defender, CoreSkillGrowthConfigPatch.XiaoZongYueGong) ||
            defender.GetMobilityValue() == 0)
        {
            return damageValue;
        }

        int availableUnits = defender.GetMobilityValue() * 100 / MoveSpecialConstants.MaxMobility + 1;
        int damageUnit = Math.Max(defender.GetDamageStepCollection().FatalDamageStep / 10, 1);
        int consumedUnits = Math.Min(damageValue / damageUnit, availableUnits);
        int mobilityCost = -MoveSpecialConstants.MaxMobility * consumedUnits / 100;

        if (mobilityCost != 0)
        {
            DomainManager.Combat.ChangeMobilityValue(context, defender, mobilityCost, changedByEffect: true, defender);
        }

        ShowEffectTip(defender, CoreSkillGrowthConfigPatch.XiaoZongEffectId);
        return damageUnit * (damageValue / damageUnit - consumedUnits);
    }

    internal static void ApplyShuiHuoInjuryRedistribution(DataContext context, CombatCharacter combatChar)
    {
        if (combatChar == null ||
            combatChar.GetInjuries().GetSum() <= 0 ||
            !CanAffectSkill(combatChar, CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong))
        {
            return;
        }

        Injuries injuries = combatChar.GetInjuries();
        Injuries oldInjuries = combatChar.GetOldInjuries();

        for (sbyte bodyPart = 0; bodyPart < 7; bodyPart++)
        {
            if (!combatChar.HasBreakInjury(bodyPart))
            {
                continue;
            }

            var (outer, inner) = injuries.Get(bodyPart);
            var (oldOuter, oldInner) = oldInjuries.Get(bodyPart);
            injuries.Change(
                bodyPart,
                isInnerInjury: false,
                (sbyte)(-Math.Min(Math.Max(outer - oldOuter, 0), ShuiHuoReduceInjuryCount)));
            injuries.Change(
                bodyPart,
                isInnerInjury: true,
                (sbyte)(-Math.Min(Math.Max(inner - oldInner, 0), ShuiHuoReduceInjuryCount)));
        }

        Injuries newInjuries = injuries.Subtract(oldInjuries);
        List<(bool inner, bool old)> injuryPool = new List<(bool inner, bool old)>();

        for (sbyte bodyPart = 0; bodyPart < 7; bodyPart++)
        {
            var (oldOuter, oldInner) = oldInjuries.Get(bodyPart);
            var (newOuter, newInner) = newInjuries.Get(bodyPart);

            AddInjuriesToPool(injuryPool, oldOuter, inner: false, old: true);
            AddInjuriesToPool(injuryPool, oldInner, inner: true, old: true);
            AddInjuriesToPool(injuryPool, newOuter, inner: false, old: false);
            AddInjuriesToPool(injuryPool, newInner, inner: true, old: false);
        }

        int perPartCount = injuryPool.Count / 7;
        int remainingCount = injuryPool.Count % 7;
        injuries.Initialize();
        oldInjuries.Initialize();

        for (sbyte bodyPart = 0; bodyPart < 7; bodyPart++)
        {
            for (int i = 0; i < perPartCount; i++)
            {
                AllocateRandomInjury(context, injuryPool, bodyPart, ref injuries, ref oldInjuries);
            }

            if (remainingCount > 0 && context.Random.Next(0, 7 - bodyPart) < remainingCount)
            {
                remainingCount--;
                AllocateRandomInjury(context, injuryPool, bodyPart, ref injuries, ref oldInjuries);
            }
        }

        combatChar.SetOldInjuries(oldInjuries, context);
        combatChar.SetInjuries(context, injuries);
        DomainManager.Combat.UpdateBodyDefeatMark(context, combatChar);
        ShowEffectTip(combatChar, CoreSkillGrowthConfigPatch.ShuiHuoEffectId);
    }

    private static void AddInjuriesToPool(List<(bool inner, bool old)> injuryPool, int count, bool inner, bool old)
    {
        for (int i = 0; i < count; i++)
        {
            injuryPool.Add((inner, old));
        }
    }

    private static void AllocateRandomInjury(
        DataContext context,
        List<(bool inner, bool old)> injuryPool,
        sbyte bodyPart,
        ref Injuries injuries,
        ref Injuries oldInjuries)
    {
        if (injuryPool.Count <= 0)
        {
            return;
        }

        int index = context.Random.Next(0, injuryPool.Count);
        var entry = injuryPool[index];
        int lastIndex = injuryPool.Count - 1;
        injuryPool[index] = injuryPool[lastIndex];
        injuryPool.RemoveAt(lastIndex);

        injuries.Change(bodyPart, entry.inner, 1);
        if (entry.old)
        {
            oldInjuries.Change(bodyPart, entry.inner, 1);
        }
    }

    private static void ShowEffectTip(CombatCharacter combatChar, short effectId)
    {
        if (combatChar != null && effectId >= 0)
        {
            DomainManager.Combat.ShowSpecialEffectTips(combatChar.GetId(), effectId, 0);
        }
    }
}

[HarmonyPatch]
internal static class XiaoZongDirectDamagePatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(CombatDomain),
            "CalcSingleInjury",
            new[]
            {
                typeof(CombatContext),
                typeof(long),
                typeof(int),
                typeof(bool),
                typeof(EDamageType),
                typeof(int),
                typeof(short),
                typeof(CValuePercentBonus),
                typeof(int)
            });
    }

    [HarmonyPrepare]
    private static bool Prepare()
    {
        MethodBase target = TargetMethod();
        if (target != null)
        {
            return true;
        }

        Console.WriteLine("[XuanShuFourArts] XiaoZong DR damage conversion skipped: CalcSingleInjury target not found.");
        return false;
    }

    [HarmonyPostfix]
    private static void ConvertDirectDamageToMobility(
        CombatContext context,
        int injuryStep,
        bool inner,
        EDamageType damageType,
        int originDamageValue,
        short combatSkillId,
        ref ValueTuple<int, int, int> __result)
    {
        try
        {
            if (damageType != EDamageType.Direct || __result.Item3 <= 0 || context.BodyPart < 0)
            {
                return;
            }

            int convertedDamage = DecoupledCoreSkillEffects.ApplyXiaoZongDirectDamageConversion(context, __result.Item3);
            if (convertedDamage == __result.Item3)
            {
                return;
            }

            int currentMarks = context.Defender.GetInjuries().Get(context.BodyPart, inner);
            int maxMarks = Math.Max(6 - currentMarks, 0);
            var (markCount, leftDamage) = CMath.CalcMarkAndLeftDamage(
                Math.Min(originDamageValue + convertedDamage, int.MaxValue),
                Math.Max(injuryStep, 1),
                maxMarks);

            if (markCount > 0 &&
                currentMarks == 0 &&
                !DomainManager.SpecialEffect.ModifyData(
                    context.AttackerId,
                    combatSkillId,
                    80,
                    dataValue: true,
                    inner ? 1 : 0))
            {
                markCount = 0;
            }

            __result = new ValueTuple<int, int, int>(markCount, leftDamage, convertedDamage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] XiaoZong DR damage conversion failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatCharacter), "SetAffectingDefendSkillId")]
internal static class ShuiHuoDefendEndPatch
{
    [HarmonyPrefix]
    private static void RedistributeInjuriesOnDefendEnd(
        CombatCharacter __instance,
        short affectingDefendSkillId,
        DataContext context)
    {
        try
        {
            if (__instance == null ||
                __instance.GetAffectingDefendSkillId() != CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong ||
                affectingDefendSkillId == CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong)
            {
                return;
            }

            DecoupledCoreSkillEffects.ApplyShuiHuoInjuryRedistribution(context, __instance);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] ShuiHuo DR injury redistribution failed: {ex}");
        }
    }
}
