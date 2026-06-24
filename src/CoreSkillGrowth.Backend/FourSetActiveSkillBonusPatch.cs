using System;
using CoreSkillGrowth.Shared;
using GameData.Combat.Math;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

internal static class FourSetActiveSkillBonus
{
    internal const int ActiveSkillBonusPercent = 40;

    internal static bool HasFullSetEquipped(int characterId)
    {
        return HasFullSetEquipped(DomainManager.Character.GetElement_Objects(characterId));
    }

    internal static bool HasFullSetEquipped(Character character)
    {
        return character != null &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.PeiRanJue) &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.XiaoZongYueGong) &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong) &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.TaiZuChangQuan);
    }

    internal static bool IsAffectingSetMoveSkill(CombatCharacter combatChar)
    {
        return combatChar != null &&
            combatChar.GetAffectingMoveSkillId() == CoreSkillGrowthConfigPatch.XiaoZongYueGong &&
            HasFullSetEquipped(combatChar.GetId());
    }

    internal static bool IsAffectingSetDefendSkill(CombatCharacter combatChar)
    {
        return combatChar != null &&
            combatChar.GetAffectingDefendSkillId() == CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong &&
            HasFullSetEquipped(combatChar.GetId());
    }
}

[HarmonyPatch(typeof(CombatSkillDomain), "CalcCastAddPercentMoveSpeed")]
internal static class FourSetActiveMoveSpeedPatch
{
    [HarmonyPostfix]
    private static void DoubleActiveMoveSkillSpeedBonus(CombatSkill skill, ref short __result)
    {
        try
        {
            if (skill == null || __result <= 0)
            {
                return;
            }

            CombatSkillKey skillKey = skill.GetId();
            if (skillKey.SkillTemplateId != CoreSkillGrowthConfigPatch.XiaoZongYueGong ||
                !DomainManager.Combat.IsCharInCombat(skillKey.CharId))
            {
                return;
            }

            CombatCharacter combatChar = DomainManager.Combat.GetElement_CombatCharacterDict(skillKey.CharId);
            if (!FourSetActiveSkillBonus.IsAffectingSetMoveSkill(combatChar))
            {
                return;
            }

            __result = (short)Math.Clamp(
                __result + __result * FourSetActiveSkillBonus.ActiveSkillBonusPercent / 100,
                short.MinValue,
                short.MaxValue);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set active move skill speed failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatCharacter), "CalcDefendSkillAddAvoidValue", new Type[]
{
    typeof(sbyte),
    typeof(bool)
})]
internal static class FourSetActiveDefendAvoidPatch
{
    [HarmonyPostfix]
    private static void DoubleActiveDefendAvoidBonus(CombatCharacter __instance, bool ignoreDefendSkill, ref int __result)
    {
        try
        {
            if (ignoreDefendSkill || __result <= 0 || !FourSetActiveSkillBonus.IsAffectingSetDefendSkill(__instance))
            {
                return;
            }

            __result += __result * FourSetActiveSkillBonus.ActiveSkillBonusPercent / 100;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set active defend avoid failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(CombatCharacter), "CalcDefendSkillAddPenetrateResistValue", new Type[]
{
    typeof(bool),
    typeof(bool)
})]
internal static class FourSetActiveDefendPenetrateResistPatch
{
    [HarmonyPostfix]
    private static void DoubleActiveDefendPenetrateResistBonus(
        CombatCharacter __instance,
        bool ignoreDefendSkill,
        ref int __result)
    {
        try
        {
            if (ignoreDefendSkill || __result <= 0 || !FourSetActiveSkillBonus.IsAffectingSetDefendSkill(__instance))
            {
                return;
            }

            __result += __result * FourSetActiveSkillBonus.ActiveSkillBonusPercent / 100;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set active defend penetrate resist failed: {ex}");
        }
    }
}
