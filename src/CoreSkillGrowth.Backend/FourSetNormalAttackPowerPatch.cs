using System;
using System.Reflection;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

[HarmonyPatch]
internal static class FourSetNormalAttackPowerPatch
{
    private const int NormalAttackPowerMultiplier = 2;

    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(CombatDomain),
            "CalcAndAddInjury",
            new[]
            {
                typeof(CombatContext),
                typeof(sbyte),
                typeof(int).MakeByRefType(),
                typeof(bool).MakeByRefType(),
                typeof(int),
                typeof(int),
                typeof(int)
            });
    }

    [HarmonyPrefix]
    private static void AmplifyNormalAttackPower(CombatContext context, ref int power)
    {
        try
        {
            if (context.AttackType != CFormula.EAttackType.Normal ||
                context.IsFightBack ||
                !HasFullSetEquipped(context.AttackerId))
            {
                return;
            }

            power *= NormalAttackPowerMultiplier;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set normal attack power failed: {ex}");
        }
    }

    private static bool HasFullSetEquipped(int characterId)
    {
        Character character = DomainManager.Character.GetElement_Objects(characterId);
        return character != null &&
            character.IsCombatSkillEquipped(0) &&
            character.IsCombatSkillEquipped(1) &&
            character.IsCombatSkillEquipped(2) &&
            character.IsCombatSkillEquipped(3);
    }
}
