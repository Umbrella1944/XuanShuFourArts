using System;
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
    private const int MajorCombatPropertyBonusPercent = 15;

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
                !IsMajorCombatProperty(propertyType) ||
                (valueSumType != EDataSumType.All && valueSumType != EDataSumType.OnlyAdd) ||
                !FourSetActiveSkillBonus.HasFullSetEquipped(__instance))
            {
                return;
            }

            __result = __result.ChangeB(MajorCombatPropertyBonusPercent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set bonus failed: {ex}");
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
