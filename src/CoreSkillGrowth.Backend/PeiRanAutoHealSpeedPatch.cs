using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CoreSkillGrowth.Shared;
using GameData.Domains;
using GameData.Domains.Character;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

internal static class PeiRanAutoHealThrottleStore
{
    private const short OfficialNewInjurySpeed = 2;
    private const short ActiveSpeed = 1;
    private const short SuppressedSpeed = 0;
    private const int BaseTicksPerActiveHeal = 10;
    private const int FullSetTicksPerActiveHeal = 6;
    private const short PeiRanJue = 0;
    private const short XiaoZongYueGong = 1;
    private const short ShuiHuoYingQiGong = 2;
    private const short TaiZuChangQuan = 3;

    private static readonly Type LoongWoodImplementHealType =
        AccessTools.TypeByName("GameData.Domains.SpecialEffect.Animal.Loong.Neigong.LoongWoodImplementHeal");

    private static readonly Type CombatSkillEffectBaseType =
        AccessTools.TypeByName("GameData.Domains.SpecialEffect.CombatSkill.CombatSkillEffectBase");

    private static readonly Type SpecialEffectBaseType =
        AccessTools.TypeByName("GameData.Domains.SpecialEffect.SpecialEffectBase");

    private static readonly Type CombatCharacterType =
        AccessTools.TypeByName("GameData.Domains.Combat.CombatCharacter");

    internal static readonly PropertyInfo EffectBaseProperty =
        AccessTools.Property(LoongWoodImplementHealType, "EffectBase");

    private static readonly PropertyInfo EffectIdProperty =
        AccessTools.Property(CombatSkillEffectBaseType, "EffectId");

    private static readonly PropertyInfo CombatCharProperty =
        AccessTools.Property(SpecialEffectBaseType, "CombatChar");

    internal static readonly FieldInfo InnerNewSpeedsField =
        AccessTools.Field(CombatCharacterType, "InnerInjuryAutoHealSpeeds");

    internal static readonly FieldInfo OuterNewSpeedsField =
        AccessTools.Field(CombatCharacterType, "OuterInjuryAutoHealSpeeds");

    internal static readonly FieldInfo InnerOldSpeedsField =
        AccessTools.Field(CombatCharacterType, "InnerOldInjuryAutoHealSpeeds");

    internal static readonly FieldInfo OuterOldSpeedsField =
        AccessTools.Field(CombatCharacterType, "OuterOldInjuryAutoHealSpeeds");

    private static readonly FieldInfo CombatCharacterIdField =
        AccessTools.Field(CombatCharacterType, "_id");

    private static readonly ConditionalWeakTable<object, HealThrottleState> States = new();

    internal static void RegisterIfPeiRanEffect(object implement)
    {
        object effectBase = EffectBaseProperty?.GetValue(implement);
        if (effectBase == null)
        {
            return;
        }

        int effectId = Convert.ToInt32(EffectIdProperty.GetValue(effectBase));
        if (!CoreSkillGrowthConfigPatch.IsPeiRanCustomEffectId(effectId))
        {
            return;
        }

        object combatChar = CombatCharProperty.GetValue(effectBase);
        if (combatChar == null)
        {
            return;
        }

        HealThrottleState state = new()
        {
            InnerNewIndex = ReplaceLastSpeed(GetSpeeds(InnerNewSpeedsField, combatChar), OfficialNewInjurySpeed, ActiveSpeed),
            OuterNewIndex = ReplaceLastSpeed(GetSpeeds(OuterNewSpeedsField, combatChar), OfficialNewInjurySpeed, ActiveSpeed),
            InnerOldIndex = FindLastSpeed(GetSpeeds(InnerOldSpeedsField, combatChar), ActiveSpeed),
            OuterOldIndex = FindLastSpeed(GetSpeeds(OuterOldSpeedsField, combatChar), ActiveSpeed)
        };

        if (!state.IsComplete)
        {
            return;
        }

        States.Remove(combatChar);
        States.Add(combatChar, state);
    }

    internal static void BeforeAutoHealUpdate(object combatChar)
    {
        if (combatChar == null || !States.TryGetValue(combatChar, out HealThrottleState state))
        {
            return;
        }

        state.DisabledForThisTick = false;
        state.TickCounter++;
        int ticksPerActiveHeal = GetTicksPerActiveHeal(combatChar);
        if (state.TickCounter % ticksPerActiveHeal == 0)
        {
            EnsureSpeed(combatChar, state, ActiveSpeed);
            return;
        }

        EnsureSpeed(combatChar, state, SuppressedSpeed);
        state.DisabledForThisTick = true;
    }

    private static int GetTicksPerActiveHeal(object combatChar)
    {
        try
        {
            int characterId = Convert.ToInt32(CombatCharacterIdField?.GetValue(combatChar));
            Character character = DomainManager.Character.GetElement_Objects(characterId);
            if (character != null &&
                character.IsCombatSkillEquipped(PeiRanJue) &&
                character.IsCombatSkillEquipped(XiaoZongYueGong) &&
                character.IsCombatSkillEquipped(ShuiHuoYingQiGong) &&
                character.IsCombatSkillEquipped(TaiZuChangQuan))
            {
                return FullSetTicksPerActiveHeal;
            }
        }
        catch
        {
            // Keep the base interval if combat character lookup is unavailable.
        }

        return BaseTicksPerActiveHeal;
    }

    internal static void AfterAutoHealUpdate(object combatChar)
    {
        if (combatChar == null || !States.TryGetValue(combatChar, out HealThrottleState state))
        {
            return;
        }

        if (!state.DisabledForThisTick)
        {
            return;
        }

        EnsureSpeed(combatChar, state, ActiveSpeed);
        state.DisabledForThisTick = false;
    }

    private static void EnsureSpeed(object combatChar, HealThrottleState state, short speed)
    {
        SetSlot(GetSpeeds(InnerNewSpeedsField, combatChar), state.InnerNewIndex, speed);
        SetSlot(GetSpeeds(OuterNewSpeedsField, combatChar), state.OuterNewIndex, speed);
        SetSlot(GetSpeeds(InnerOldSpeedsField, combatChar), state.InnerOldIndex, speed);
        SetSlot(GetSpeeds(OuterOldSpeedsField, combatChar), state.OuterOldIndex, speed);
    }

    private static List<short> GetSpeeds(FieldInfo field, object combatChar)
    {
        return field?.GetValue(combatChar) as List<short>;
    }

    private static int ReplaceLastSpeed(List<short> speeds, short oldSpeed, short newSpeed)
    {
        if (speeds == null)
        {
            return -1;
        }

        for (int i = speeds.Count - 1; i >= 0; i--)
        {
            if (speeds[i] == oldSpeed)
            {
                speeds[i] = newSpeed;
                return i;
            }
        }

        return -1;
    }

    private static int FindLastSpeed(List<short> speeds, short speed)
    {
        if (speeds == null)
        {
            return -1;
        }

        for (int i = speeds.Count - 1; i >= 0; i--)
        {
            if (speeds[i] == speed)
            {
                return i;
            }
        }

        return -1;
    }

    private static void SetSlot(List<short> speeds, int index, short speed)
    {
        if (speeds == null || index < 0 || index >= speeds.Count)
        {
            return;
        }

        short current = speeds[index];
        if (current == ActiveSpeed || current == SuppressedSpeed)
        {
            speeds[index] = speed;
        }
    }

    private sealed class HealThrottleState
    {
        public int InnerNewIndex = -1;
        public int OuterNewIndex = -1;
        public int InnerOldIndex = -1;
        public int OuterOldIndex = -1;
        public int TickCounter;
        public bool DisabledForThisTick;

        public bool IsComplete =>
            InnerNewIndex >= 0 &&
            OuterNewIndex >= 0 &&
            InnerOldIndex >= 0 &&
            OuterOldIndex >= 0;
    }
}

[HarmonyPatch]
internal static class PeiRanAutoHealRegisterPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            AccessTools.TypeByName("GameData.Domains.SpecialEffect.Animal.Loong.Neigong.LoongWoodImplementHeal"),
            "OnCombatBegin");
    }

    [HarmonyPostfix]
    private static void RegisterThrottle(object __instance)
    {
        try
        {
            PeiRanAutoHealThrottleStore.RegisterIfPeiRanEffect(__instance);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] failed to register PeiRan auto-heal throttle: {ex}");
        }
    }
}

[HarmonyPatch]
internal static class PeiRanAutoHealUpdatePatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            AccessTools.TypeByName("GameData.Domains.Combat.CombatCharacterStateBase"),
            "TimeUpdateAutoHeal",
            new[]
            {
                AccessTools.TypeByName("GameData.Common.DataContext"),
                AccessTools.TypeByName("GameData.Domains.Combat.CombatCharacter")
            });
    }

    [HarmonyPrefix]
    private static void BeforeAutoHealUpdate(object combatChar)
    {
        try
        {
            PeiRanAutoHealThrottleStore.BeforeAutoHealUpdate(combatChar);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] failed before PeiRan auto-heal update: {ex}");
        }
    }

    [HarmonyPostfix]
    private static void AfterAutoHealUpdate(object combatChar)
    {
        try
        {
            PeiRanAutoHealThrottleStore.AfterAutoHealUpdate(combatChar);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] failed after PeiRan auto-heal update: {ex}");
        }
    }
}
