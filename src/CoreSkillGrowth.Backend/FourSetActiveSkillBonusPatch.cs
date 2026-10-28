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
    internal static int ActiveSkillBonusPercent => StrengthSettings.SetActiveBonus;

    // Growth-scaled four-piece active bonus for a concrete wielder.
    internal static int ActiveBonusFor(int charId)
    {
        return GrowthSettings.ScaleByFactor(ActiveSkillBonusPercent, GrowthSettings.FactorForCharacter(charId));
    }

    // Scale a baked skill value (move speed / avoid / penetrate resist that the
    // config patch wrote at slider strength) by the wielder's growth factor.
    internal static int ScaleSkillBase(int bakedValue, int charId)
    {
        return GrowthSettings.ScaleByFactor(bakedValue, GrowthSettings.FactorForCharacter(charId));
    }

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
            bool fullSet = FourSetActiveSkillBonus.IsAffectingSetMoveSkill(combatChar);
            // Grow the art's own baked cast bonus whenever it is the active move
            // skill; the set bonus stacks on top only with the full set.
            if (combatChar != null && combatChar.GetAffectingMoveSkillId() == CoreSkillGrowthConfigPatch.XiaoZongYueGong)
            {
                __result = (short)Math.Clamp(
                    FourSetActiveSkillBonus.ScaleSkillBase(__result, skillKey.CharId),
                    short.MinValue,
                    short.MaxValue);
            }

            if (!fullSet)
            {
                return;
            }

            __result = (short)Math.Clamp(
                StrengthSettings.Amplify(__result, FourSetActiveSkillBonus.ActiveBonusFor(skillKey.CharId)),
                short.MinValue,
                short.MaxValue);
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] four-set active move skill speed failed: {ex}");
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
            if (__result <= 0)
            {
                return;
            }

            // Grow the art's own baked defend bonus whenever it is active; the
            // set bonus stacks on top only with the full set.
            if (__instance.GetAffectingDefendSkillId() == CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong)
            {
                __result = FourSetActiveSkillBonus.ScaleSkillBase(__result, __instance.GetId());
            }

            if (ignoreDefendSkill || !FourSetActiveSkillBonus.IsAffectingSetDefendSkill(__instance))
            {
                return;
            }

            __result = StrengthSettings.Amplify(__result, FourSetActiveSkillBonus.ActiveBonusFor(__instance.GetId()));
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] four-set active defend avoid failed: {ex}");
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
            if (__result <= 0)
            {
                return;
            }

            // Grow the art's own baked defend bonus whenever it is active; the
            // set bonus stacks on top only with the full set.
            if (__instance.GetAffectingDefendSkillId() == CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong)
            {
                __result = FourSetActiveSkillBonus.ScaleSkillBase(__result, __instance.GetId());
            }

            if (ignoreDefendSkill || !FourSetActiveSkillBonus.IsAffectingSetDefendSkill(__instance))
            {
                return;
            }

            __result = StrengthSettings.Amplify(__result, FourSetActiveSkillBonus.ActiveBonusFor(__instance.GetId()));
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] four-set active defend penetrate resist failed: {ex}");
        }
    }
}
