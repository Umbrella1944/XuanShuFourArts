using System;
using System.Reflection;
using CoreSkillGrowth.Shared;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.Animal.Beast.Neigong;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

[HarmonyPatch(typeof(WanShouZhiWang), "GetModifyValue")]
internal static class TaiZuComboAmplifyPatch
{
    private const ushort ComboDamageFieldId = 69;
    private const ushort ComboHitPenaltyFieldId = 74;
    private const int DamageBonusPerCombo = 150;
    private const int MaxDamageBonus = 250;
    private const int HitPenaltyPerCombo = -15;

    private static readonly FieldInfo PerpetualAttackCountField =
        AccessTools.Field(typeof(WanShouZhiWang), "_perpetualAttackCount");

    [HarmonyPostfix]
    private static void AmplifyFullSetCombo(WanShouZhiWang __instance, AffectedDataKey dataKey, ref int __result)
    {
        try
        {
            if (__instance == null ||
                !CoreSkillGrowthConfigPatch.IsTaiZuCustomEffectId(__instance.EffectId) ||
                !HasFullSetEquipped(__instance.CharacterId))
            {
                return;
            }

            int comboCount = Convert.ToInt32(PerpetualAttackCountField.GetValue(__instance));
            if (comboCount <= 0)
            {
                __result = 0;
                return;
            }

            if (dataKey.FieldId == ComboDamageFieldId)
            {
                __result = Math.Min(comboCount * DamageBonusPerCombo, MaxDamageBonus);
                return;
            }

            if (dataKey.FieldId == ComboHitPenaltyFieldId)
            {
                __result = comboCount * HitPenaltyPerCombo;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] TaiZu combo amplify failed: {ex}");
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
