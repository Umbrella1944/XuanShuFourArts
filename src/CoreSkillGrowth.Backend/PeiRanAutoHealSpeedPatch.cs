using System;
using System.Collections.Generic;
using System.Reflection;
using CoreSkillGrowth.Shared;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

internal static class PeiRanAutoHealRuntime
{
    private const short ActiveSpeed = 1;
    private const short SuppressedSpeed = 0;
    private const int BaseTicksPerActiveHeal = 10;
    private const int FullSetTicksPerActiveHeal = 7;

    private static readonly Dictionary<int, HealThrottleState> States = new Dictionary<int, HealThrottleState>();

    internal static void RegisterCombatBegin()
    {
        States.Clear();

        foreach (int charId in DomainManager.Combat.GetTeamCharacterIds())
        {
            if (!DomainManager.Combat.TryGetElement_CombatCharacterDict(charId, out CombatCharacter combatChar) ||
                !combatChar.GetCharacter().IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.PeiRanJue) ||
                !DecoupledCoreSkillEffects.CanAffectSkill(combatChar, CoreSkillGrowthConfigPatch.PeiRanJue))
            {
                continue;
            }

            HealThrottleState state = new HealThrottleState
            {
                InnerNewIndex = AddSpeed(combatChar.InnerInjuryAutoHealSpeeds, ActiveSpeed),
                OuterNewIndex = AddSpeed(combatChar.OuterInjuryAutoHealSpeeds, ActiveSpeed),
                InnerOldIndex = AddSpeed(combatChar.InnerOldInjuryAutoHealSpeeds, ActiveSpeed),
                OuterOldIndex = AddSpeed(combatChar.OuterOldInjuryAutoHealSpeeds, ActiveSpeed)
            };

            if (state.IsComplete)
            {
                States[combatChar.GetId()] = state;
            }
        }
    }

    internal static void ClearAll()
    {
        States.Clear();
    }

    internal static void BeforeAutoHealUpdate(CombatCharacter combatChar)
    {
        if (combatChar == null || !States.TryGetValue(combatChar.GetId(), out HealThrottleState state))
        {
            return;
        }

        state.DisabledForThisTick = false;
        state.TickCounter++;
        int ticksPerActiveHeal = FourSetActiveSkillBonus.HasFullSetEquipped(combatChar.GetId())
            ? FullSetTicksPerActiveHeal
            : BaseTicksPerActiveHeal;

        if (state.TickCounter % ticksPerActiveHeal == 0)
        {
            EnsureSpeed(combatChar, state, ActiveSpeed);
            return;
        }

        EnsureSpeed(combatChar, state, SuppressedSpeed);
        state.DisabledForThisTick = true;
    }

    internal static void AfterAutoHealUpdate(CombatCharacter combatChar)
    {
        if (combatChar == null ||
            !States.TryGetValue(combatChar.GetId(), out HealThrottleState state) ||
            !state.DisabledForThisTick)
        {
            return;
        }

        EnsureSpeed(combatChar, state, ActiveSpeed);
        state.DisabledForThisTick = false;
    }

    private static int AddSpeed(List<short> speeds, short speed)
    {
        if (speeds == null)
        {
            return -1;
        }

        speeds.Add(speed);
        return speeds.Count - 1;
    }

    private static void EnsureSpeed(CombatCharacter combatChar, HealThrottleState state, short speed)
    {
        SetSlot(combatChar.InnerInjuryAutoHealSpeeds, state.InnerNewIndex, speed);
        SetSlot(combatChar.OuterInjuryAutoHealSpeeds, state.OuterNewIndex, speed);
        SetSlot(combatChar.InnerOldInjuryAutoHealSpeeds, state.InnerOldIndex, speed);
        SetSlot(combatChar.OuterOldInjuryAutoHealSpeeds, state.OuterOldIndex, speed);
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

[HarmonyPatch(typeof(Events), "RaiseCombatBegin")]
internal static class PeiRanCombatBeginPatch
{
    [HarmonyPostfix]
    private static void RegisterAutoHeal()
    {
        try
        {
            PeiRanAutoHealRuntime.RegisterCombatBegin();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] failed to register PeiRan DR auto-heal: {ex}");
        }
    }
}

[HarmonyPatch(typeof(Events), "RaiseCombatSettlement")]
internal static class PeiRanCombatSettlementPatch
{
    [HarmonyPostfix]
    private static void ClearRuntime()
    {
        PeiRanAutoHealRuntime.ClearAll();
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
                typeof(DataContext),
                typeof(CombatCharacter)
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

        Console.WriteLine("[XuanShuFourArts] PeiRan DR auto-heal patch skipped: TimeUpdateAutoHeal target not found.");
        return false;
    }

    [HarmonyPrefix]
    private static void BeforeAutoHealUpdate(CombatCharacter combatChar)
    {
        try
        {
            PeiRanAutoHealRuntime.BeforeAutoHealUpdate(combatChar);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] failed before PeiRan DR auto-heal update: {ex}");
        }
    }

    [HarmonyPostfix]
    private static void AfterAutoHealUpdate(CombatCharacter combatChar)
    {
        try
        {
            PeiRanAutoHealRuntime.AfterAutoHealUpdate(combatChar);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] failed after PeiRan DR auto-heal update: {ex}");
        }
    }
}
