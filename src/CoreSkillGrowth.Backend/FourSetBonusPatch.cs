using System;
using CoreSkillGrowth.Shared;
using GameData.Combat.Math;
using GameData.Domains.Character;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

[HarmonyPatch(typeof(Character), "CalcPropertyModify", new Type[]
{
    typeof(ECharacterPropertyReferencedType),
    typeof(EDataSumType)
})]
internal static class FourSetBonusPatch
{
    private static readonly short[] CoreSkillIds =
    {
        CoreSkillGrowthConfigPatch.PeiRanJue,
        CoreSkillGrowthConfigPatch.XiaoZongYueGong,
        CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong,
        CoreSkillGrowthConfigPatch.TaiZuChangQuan
    };

    [HarmonyPostfix]
    private static void AddFourSetBonus(
        Character __instance,
        ECharacterPropertyReferencedType propertyType,
        EDataSumType valueSumType,
        ref CValueModify __result)
    {
        try
        {
            if (__instance == null ||
                (valueSumType != EDataSumType.All && valueSumType != EDataSumType.OnlyAdd))
            {
                return;
            }

            short propertyId = (short)propertyType;
            int equippedMask = 0;
            for (int i = 0; i < CoreSkillIds.Length; i++)
            {
                if (__instance.IsCombatSkillEquipped(CoreSkillIds[i]))
                {
                    equippedMask |= 1 << i;
                }
            }

            if (equippedMask == 0)
            {
                return;
            }

            // One factor per wielder; scales both the four-piece resonance and
            // the per-art practice-bonus delta injected below.
            int factor = GrowthSettings.FactorForCharacter(__instance);
            if ((equippedMask & 1) != 0 &&
                (propertyType == ECharacterPropertyReferencedType.RecoveryOfStance ||
                 propertyType == ECharacterPropertyReferencedType.RecoveryOfBreath))
            {
                int baked = StrengthSettings.ScaleShort(4, StrengthSettings.ReturningFrame);
                __result = __result.ChangeA(GrowthSettings.DeltaFromNeutral(baked, factor));
            }

            if (factor == GrowthSettings.DisabledFactor)
            {
                if (FourSetActiveSkillBonus.HasFullSetEquipped(__instance) && IsMajorCombatProperty(propertyType))
                {
                    __result = __result.ChangeB(StrengthSettings.SetCombatBonus);
                }
                return;
            }

            if (FourSetActiveSkillBonus.HasFullSetEquipped(__instance) && IsMajorCombatProperty(propertyType))
            {
                __result = __result.ChangeB(
                    GrowthSettings.ScaleByFactor(StrengthSettings.SetCombatBonus, factor));
            }

            // Practice bonuses were baked growth-neutral (slider x 100%); add the
            // per-wielder delta so equippers see exactly slider x factor%.
            for (int i = 0; i < CoreSkillIds.Length; i++)
            {
                if ((equippedMask & (1 << i)) == 0)
                {
                    continue;
                }

                int slider = StrengthSettings.ForSkill(CoreSkillIds[i]);
                (short PropertyId, short Value)[] table = CoreSkillGrowthConfigPatch.CoreSkillPropertyTables[i];
                for (int j = 0; j < table.Length; j++)
                {
                    if (table[j].PropertyId != propertyId)
                    {
                        continue;
                    }

                    int baked = StrengthSettings.Scale(table[j].Value, slider);
                    __result = __result.ChangeA(GrowthSettings.DeltaFromNeutral(baked, factor));
                }
            }
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] four-set bonus failed: {ex}");
        }
    }

    private static bool IsMajorCombatProperty(ECharacterPropertyReferencedType propertyType)
    {
        switch (propertyType)
        {
            case ECharacterPropertyReferencedType.PenetrateOfOuter:
            case ECharacterPropertyReferencedType.PenetrateOfInner:
            case ECharacterPropertyReferencedType.PenetrateResistOfOuter:
            case ECharacterPropertyReferencedType.PenetrateResistOfInner:
            case ECharacterPropertyReferencedType.HitRateStrength:
            case ECharacterPropertyReferencedType.HitRateTechnique:
            case ECharacterPropertyReferencedType.HitRateSpeed:
            case ECharacterPropertyReferencedType.HitRateMind:
            case ECharacterPropertyReferencedType.AvoidRateStrength:
            case ECharacterPropertyReferencedType.AvoidRateTechnique:
            case ECharacterPropertyReferencedType.AvoidRateSpeed:
            case ECharacterPropertyReferencedType.AvoidRateMind:
                return true;
            default:
                return false;
        }
    }
}
