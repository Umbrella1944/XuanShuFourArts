using System;
using System.Collections.Generic;
using System.Reflection;
using CoreSkillGrowth.Shared;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using HarmonyLib;

namespace XuanShuFourArts.Backend;

internal sealed class BloodRiftCombatState
{
    public int FailureCount;
    public int Stacks;
    public bool PendingFreeAttack;
    public bool FreeAttackActive;
    public int FreeAttackSnapshotStacks;
    public bool FreeAttackDamageSettled;
    public bool RollAttemptedInCurrentNormalAttack;

    public void ClearFreeAttack()
    {
        PendingFreeAttack = false;
        FreeAttackActive = false;
        FreeAttackSnapshotStacks = 0;
        FreeAttackDamageSettled = false;
    }

    public void ResetNormalAttackRoll()
    {
        RollAttemptedInCurrentNormalAttack = false;
    }
}

internal static class BloodRiftPursuitRuntime
{
    private static readonly bool BloodRiftTestMode = false;
    private const int BaseChancePercent = 20;
    private const int PityStartFailures = 4;
    private const int PityStepPercent = 15;
    private const int PityMaxChancePercent = 100;
    private const int FullSetNormalAttackPowerPercent = 200;
    private const int TriggerStacks = 2;
    private const int MaxStacks = 10;
    private const int PursuitPowerPercentPerStack = 200;

    private static readonly Dictionary<int, BloodRiftCombatState> CombatStates = new Dictionary<int, BloodRiftCombatState>();

    internal static void ClearAll()
    {
        CombatStates.Clear();
    }

    internal static void OnNormalAttackEnd(
        DataContext context,
        CombatCharacter attacker,
        CombatCharacter defender,
        bool hit,
        bool isFightBack)
    {
        try
        {
            if (!CanRollPursuit(attacker, defender, hit, isFightBack))
            {
                return;
            }

            int charId = attacker.GetId();
            BloodRiftCombatState state = GetCombatState(charId);
            if (state.PendingFreeAttack || state.FreeAttackActive)
            {
                return;
            }

            if (state.RollAttemptedInCurrentNormalAttack)
            {
                return;
            }

            state.RollAttemptedInCurrentNormalAttack = true;
            bool fullSet = FourSetActiveSkillBonus.HasFullSetEquipped(charId);
            int chance = CalcChance(state, fullSet);
            if (!RollPercent(context, chance))
            {
                if (fullSet)
                {
                    state.FailureCount++;
                }

                return;
            }

            state.FailureCount = 0;
            state.Stacks = Math.Min(MaxStacks, state.Stacks + TriggerStacks);
            state.PendingFreeAttack = true;
            state.FreeAttackActive = false;
            state.FreeAttackSnapshotStacks = 0;
            state.FreeAttackDamageSettled = false;
            SyncStackDisplay(context, attacker, state);
            ShowBloodRiftTip(attacker, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] Blood-Rift pursuit trigger failed: {ex}");
        }
    }

    internal static void OnNormalAttackPrepareEnd(DataContext context, int charId)
    {
        try
        {
            if (!CombatStates.TryGetValue(charId, out BloodRiftCombatState state) ||
                !state.PendingFreeAttack)
            {
                return;
            }

            CombatCharacter combatChar = DomainManager.Combat.GetElement_CombatCharacterDict(charId);
            if (combatChar == null || HasPursuitInterruptCommand(combatChar))
            {
                LoseStack(state);
                SyncStackDisplay(context, combatChar, state);
                ShowBloodRiftTip(combatChar, 1);
                state.ClearFreeAttack();
                return;
            }

            state.PendingFreeAttack = false;
            state.FreeAttackActive = true;
            state.FreeAttackSnapshotStacks = Math.Max(state.Stacks, 0);
            state.FreeAttackDamageSettled = false;
            if (state.FreeAttackSnapshotStacks <= 0)
            {
                SyncStackDisplay(context, combatChar, state);
                state.ClearFreeAttack();
                return;
            }

            combatChar.AttackForceHitCount++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] Blood-Rift pursuit prepare failed: {ex}");
        }
    }

    internal static void OnNormalAttackAllEnd(DataContext context, CombatCharacter attacker)
    {
        try
        {
            if (attacker == null ||
                !CombatStates.TryGetValue(attacker.GetId(), out BloodRiftCombatState state))
            {
                return;
            }

            if (state.FreeAttackActive)
            {
                if (!state.FreeAttackDamageSettled)
                {
                    LoseStack(state);
                    ShowBloodRiftTip(attacker, 1);
                }

                SyncStackDisplay(context, attacker, state);
                state.ClearFreeAttack();
                state.ResetNormalAttackRoll();
                return;
            }

            if (!state.PendingFreeAttack)
            {
                state.ResetNormalAttackRoll();
                return;
            }

            if (HasPursuitInterruptCommand(attacker) || !CanStartFreeAttack(attacker))
            {
                LoseStack(state);
                SyncStackDisplay(context, attacker, state);
                ShowBloodRiftTip(attacker, 1);
                state.ClearFreeAttack();
                state.ResetNormalAttackRoll();
                return;
            }

            state.ResetNormalAttackRoll();
            attacker.NormalAttackFree();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] Blood-Rift pursuit queue failed: {ex}");
        }
    }

    internal static bool TrySuppressVanillaPursuit(CombatCharacter character, out bool result)
    {
        result = false;
        return ShouldControlPursuit(character);
    }

    internal static bool IsBloodRiftPursuitNormalAttack(CombatContext context)
    {
        CombatCharacter attacker = context.Attacker;
        return attacker != null &&
            context.IsNormalAttack &&
            !context.IsFightBack &&
            CombatStates.TryGetValue(attacker.GetId(), out BloodRiftCombatState state) &&
            state.FreeAttackActive &&
            state.FreeAttackSnapshotStacks > 0;
    }

    internal static bool ShouldAmplifyNormalAttack(CombatContext context)
    {
        if (!context.IsNormalAttack ||
            context.IsFightBack ||
            context.DamageType == EDamageType.Bounce)
        {
            return false;
        }

        CombatCharacter attacker = context.Attacker;
        if (attacker == null ||
            attacker.NormalAttackHitType < 0 ||
            !attacker.IsAlly ||
            !DomainManager.Combat.IsMainCharacter(attacker) ||
            !FourSetActiveSkillBonus.HasFullSetEquipped(attacker.GetId()))
        {
            return false;
        }

        Character character = attacker.GetCharacter();
        return character != null &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.TaiZuChangQuan);
    }

    internal static void AmplifyNormalAttackPower(ref int power)
    {
        power = Math.Max(0, power * FullSetNormalAttackPowerPercent / 100);
    }

    internal static void AmplifyBloodRiftPursuitPower(CombatContext context, ref int power)
    {
        CombatCharacter attacker = context.Attacker;
        if (!IsBloodRiftPursuitNormalAttack(context) ||
            attacker == null ||
            !CombatStates.TryGetValue(attacker.GetId(), out BloodRiftCombatState state))
        {
            return;
        }

        int multiplierPercent = 100 + state.FreeAttackSnapshotStacks * PursuitPowerPercentPerStack;
        power = Math.Max(0, power * multiplierPercent / 100);
        if (!state.FreeAttackDamageSettled)
        {
            state.Stacks = 0;
            state.FreeAttackDamageSettled = true;
            ShowBloodRiftTip(attacker, 2);
        }
    }

    internal static bool HasHigherPriorityCommand(CombatCharacter combatChar)
    {
        if (combatChar == null)
        {
            return false;
        }

        return HasPendingSkill(combatChar) ||
            combatChar.NeedShowChangeTrick ||
            combatChar.GetPreparingOtherAction() >= 0 ||
            combatChar.NeedUseOtherAction != -1 ||
            combatChar.NeedForceFlee ||
            combatChar.NeedUseItem.IsValid() ||
            combatChar.GetPreparingItem().IsValid() ||
            combatChar.NeedPauseJumpMove ||
            combatChar.NeedEnterSpecialShow ||
            combatChar.PreparingOrDoingTeammateCommand();
    }

    internal static bool HasPendingSkill(CombatCharacter combatChar)
    {
        return combatChar.GetPreparingSkillId() >= 0 ||
            combatChar.NeedUseSkillFreeId >= 0 ||
            (combatChar.NeedUseSkillId >= 0 &&
                (combatChar.GetAffectingDefendSkillId() < 0 ||
                    DomainManager.SpecialEffect.ModifyData(
                        combatChar.GetId(),
                        combatChar.NeedUseSkillId,
                        223,
                        false)));
    }

    internal static bool HasPursuitInterruptCommand(CombatCharacter combatChar)
    {
        if (combatChar == null)
        {
            return false;
        }

        return HasQueuedSkillCommand(combatChar) ||
            combatChar.NeedShowChangeTrick ||
            combatChar.NeedUseOtherAction != -1 ||
            combatChar.NeedForceFlee ||
            combatChar.NeedUseItem.IsValid() ||
            combatChar.NeedPauseJumpMove ||
            combatChar.NeedEnterSpecialShow ||
            combatChar.PreparingOrDoingTeammateCommand();
    }

    internal static void CancelPendingFreeAttackForCommand(CombatCharacter combatChar)
    {
        if (combatChar == null ||
            !CombatStates.TryGetValue(combatChar.GetId(), out BloodRiftCombatState state) ||
            !state.PendingFreeAttack ||
            !HasPursuitInterruptCommand(combatChar))
        {
            return;
        }

        LoseStack(state);
        SyncStackDisplay(DataContextManager.GetCurrentThreadDataContext(), combatChar, state);
        ShowBloodRiftTip(combatChar, 1);
        state.ClearFreeAttack();
    }

    private static bool HasQueuedSkillCommand(CombatCharacter combatChar)
    {
        return combatChar.NeedUseSkillFreeId >= 0 ||
            (combatChar.NeedUseSkillId >= 0 &&
                (combatChar.GetAffectingDefendSkillId() < 0 ||
                    DomainManager.SpecialEffect.ModifyData(
                        combatChar.GetId(),
                        combatChar.NeedUseSkillId,
                        223,
                        false)));
    }

    private static bool CanRollPursuit(
        CombatCharacter attacker,
        CombatCharacter defender,
        bool hit,
        bool isFightBack)
    {
        if (attacker == null ||
            defender == null ||
            !hit ||
            isFightBack ||
            attacker.IsAutoNormalAttackingSpecial ||
            attacker.IsBreakAttacking ||
            !attacker.IsAlly ||
            !DomainManager.Combat.IsMainCharacter(attacker) ||
            !DomainManager.Combat.IsInCombat())
        {
            return false;
        }

        Character character = attacker.GetCharacter();
        return character != null &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.TaiZuChangQuan);
    }

    private static bool ShouldControlPursuit(CombatCharacter character)
    {
        if (character == null ||
            character.IsAutoNormalAttackingSpecial ||
            character.IsBreakAttacking ||
            character.GetIsFightBack() ||
            !character.IsAlly ||
            !DomainManager.Combat.IsMainCharacter(character) ||
            !DomainManager.Combat.IsInCombat())
        {
            return false;
        }

        Character owner = character.GetCharacter();
        if (owner == null ||
            !owner.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.TaiZuChangQuan) ||
            !CombatStates.TryGetValue(character.GetId(), out BloodRiftCombatState state))
        {
            return false;
        }

        return state.FreeAttackActive &&
            state.FreeAttackSnapshotStacks > 0;
    }

    private static bool CanStartFreeAttack(CombatCharacter character)
    {
        CombatCharacter defender = DomainManager.Combat.GetCombatCharacter(!character.IsAlly, tryGetCoverCharacter: true);
        return defender != null &&
            DomainManager.Combat.IsInCombat() &&
            !character.GetIsFightBack() &&
            !defender.GetIsFightBack() &&
            defender.ChangeCharId < 0 &&
            character.ChangeCharId < 0 &&
            !defender.NeedChangeBossPhase &&
            !DomainManager.Combat.IsCharacterFallen(defender) &&
            !DomainManager.Combat.IsCharacterFallen(character);
    }

    private static BloodRiftCombatState GetCombatState(int charId)
    {
        if (!CombatStates.TryGetValue(charId, out BloodRiftCombatState state))
        {
            state = new BloodRiftCombatState();
            CombatStates.Add(charId, state);
        }

        return state;
    }

    private static int CalcChance(BloodRiftCombatState state, bool fullSet)
    {
        if (BloodRiftTestMode)
        {
            return 100;
        }

        if (!fullSet)
        {
            return BaseChancePercent;
        }

        int extra = state.FailureCount >= PityStartFailures
            ? (state.FailureCount - PityStartFailures + 1) * PityStepPercent
            : 0;
        return Math.Min(BaseChancePercent + extra, PityMaxChancePercent);
    }

    private static void LoseStack(BloodRiftCombatState state)
    {
        if (state.Stacks > 0)
        {
            state.Stacks--;
        }
    }

    private static void SyncStackDisplay(DataContext context, CombatCharacter combatChar, BloodRiftCombatState state)
    {
        if (context == null || combatChar == null)
        {
            return;
        }

        short targetCount = (short)Math.Clamp(state?.Stacks ?? 0, 0, MaxStacks);
        SkillEffectKey key = new SkillEffectKey(CoreSkillGrowthConfigPatch.TaiZuChangQuan, true);

        if (targetCount <= 0)
        {
            if (DomainManager.Combat.IsSkillEffectExist(combatChar, key))
            {
                DomainManager.Combat.RemoveSkillEffect(context, combatChar, key);
            }

            return;
        }

        if (!DomainManager.Combat.IsSkillEffectExist(combatChar, key))
        {
            DomainManager.Combat.AddSkillEffect(context, combatChar, key, targetCount, MaxStacks, autoRemoveOnNoCount: true);
            return;
        }

        short currentCount = DomainManager.Combat.GetSkillEffectCount(combatChar, key);
        short delta = (short)(targetCount - currentCount);
        if (delta != 0)
        {
            DomainManager.Combat.ChangeSkillEffectCount(context, combatChar, key, delta, forceChange: true);
        }
    }

    private static void ShowBloodRiftTip(CombatCharacter combatChar, byte index)
    {
        if (combatChar == null || CoreSkillGrowthConfigPatch.TaiZuDisplayEffectId < 0)
        {
            return;
        }

        DomainManager.Combat.ShowSpecialEffectTips(combatChar.GetId(), CoreSkillGrowthConfigPatch.TaiZuDisplayEffectId, index);
    }

    private static bool RollPercent(DataContext context, int chance)
    {
        return context.Random.Next(100) < chance;
    }
}

[HarmonyPatch(typeof(Events), "RaiseCombatBegin")]
internal static class BloodRiftCombatBeginPatch
{
    [HarmonyPostfix]
    private static void ClearRuntime()
    {
        BloodRiftPursuitRuntime.ClearAll();
    }
}

[HarmonyPatch(typeof(Events), "RaiseCombatSettlement")]
internal static class BloodRiftCombatSettlementPatch
{
    [HarmonyPostfix]
    private static void ClearRuntime()
    {
        BloodRiftPursuitRuntime.ClearAll();
    }
}

[HarmonyPatch(typeof(Events), "RaiseNormalAttackEnd")]
internal static class BloodRiftNormalAttackEndPatch
{
    [HarmonyPostfix]
    private static void TriggerBloodRiftPursuit(
        DataContext context,
        CombatCharacter attacker,
        CombatCharacter defender,
        sbyte trickType,
        int pursueIndex,
        bool hit,
        bool isFightBack)
    {
        BloodRiftPursuitRuntime.OnNormalAttackEnd(
            context,
            attacker,
            defender,
            hit,
            isFightBack);
    }
}

[HarmonyPatch(typeof(Events), "RaiseNormalAttackPrepareEnd")]
internal static class BloodRiftNormalAttackPrepareEndPatch
{
    [HarmonyPostfix]
    private static void StartBloodRiftFreeAttack(DataContext context, int charId, bool isAlly)
    {
        BloodRiftPursuitRuntime.OnNormalAttackPrepareEnd(context, charId);
    }
}

[HarmonyPatch(typeof(Events), "RaiseNormalAttackAllEnd")]
internal static class BloodRiftNormalAttackAllEndPatch
{
    [HarmonyPostfix]
    private static void QueueBloodRiftPursuit(
        DataContext context,
        CombatCharacter attacker,
        CombatCharacter defender)
    {
        BloodRiftPursuitRuntime.OnNormalAttackAllEnd(context, attacker);
    }
}

[HarmonyPatch(typeof(CombatDomain), "CanPursue")]
internal static class BloodRiftCanPursuePatch
{
    [HarmonyPrefix]
    private static bool SuppressVanillaPursuit(CombatCharacter character, bool critical, ref bool __result)
    {
        if (!BloodRiftPursuitRuntime.TrySuppressVanillaPursuit(character, out bool result))
        {
            return true;
        }

        __result = result;
        return false;
    }
}

[HarmonyPatch(typeof(CombatDomain), "AddBounceDamage", new[] { typeof(CombatContext), typeof(sbyte) })]
internal static class BloodRiftNormalAttackBouncePatch
{
    [HarmonyPrefix]
    private static bool DisableBloodRiftPursuitBounce(CombatContext context)
    {
        return !BloodRiftPursuitRuntime.IsBloodRiftPursuitNormalAttack(context);
    }
}

[HarmonyPatch(typeof(CombatContext), "CheckCritical")]
internal static class BloodRiftPursuitCriticalPatch
{
    [HarmonyPrefix]
    private static bool ForceBloodRiftPursuitCritical(CombatContext __instance, ref bool __result)
    {
        if (!BloodRiftPursuitRuntime.IsBloodRiftPursuitNormalAttack(__instance))
        {
            return true;
        }

        __result = true;
        return false;
    }
}

[HarmonyPatch]
internal static class BloodRiftNormalAttackPowerPatch
{
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

    [HarmonyPrepare]
    private static bool Prepare()
    {
        MethodBase target = TargetMethod();
        if (target != null)
        {
            return true;
        }

        Console.WriteLine("[XuanShuFourArts] Blood-Rift power patch skipped: CalcAndAddInjury target not found.");
        return false;
    }

    [HarmonyPrefix]
    private static void AmplifyBloodRiftNormalAttack(CombatContext context, ref int power)
    {
        if (BloodRiftPursuitRuntime.ShouldAmplifyNormalAttack(context))
        {
            BloodRiftPursuitRuntime.AmplifyNormalAttackPower(ref power);
        }

        BloodRiftPursuitRuntime.AmplifyBloodRiftPursuitPower(context, ref power);
    }
}

[HarmonyPatch(typeof(CombatCharacterStateMachine), "GetProperState")]
internal static class FourSetCommandPriorityPatch
{
    private static readonly FieldInfo CombatCharField =
        AccessTools.Field(typeof(CombatCharacterStateMachine), "_combatChar");

    [HarmonyPostfix]
    private static void PrioritizePlayerCommands(CombatCharacterStateMachine __instance, ref CombatCharacterStateType __result)
    {
        try
        {
            if (__result != CombatCharacterStateType.PrepareAttack)
            {
                return;
            }

            CombatCharacter combatChar = CombatCharField.GetValue(__instance) as CombatCharacter;
            if (combatChar == null ||
                !combatChar.IsAlly ||
                !DomainManager.Combat.IsMainCharacter(combatChar) ||
                !FourSetActiveSkillBonus.HasFullSetEquipped(combatChar.GetId()))
            {
                return;
            }

            BloodRiftPursuitRuntime.CancelPendingFreeAttackForCommand(combatChar);

            if (BloodRiftPursuitRuntime.HasPendingSkill(combatChar))
            {
                __result = CombatCharacterStateType.PrepareSkill;
                return;
            }

            if (combatChar.NeedShowChangeTrick && !combatChar.PreparingOrDoingTeammateCommand())
            {
                __result = CombatCharacterStateType.SelectChangeTrick;
                return;
            }

            if (combatChar.GetPreparingOtherAction() >= 0 ||
                combatChar.NeedUseOtherAction != -1 ||
                combatChar.NeedForceFlee)
            {
                __result = CombatCharacterStateType.PrepareOtherAction;
                return;
            }

            if (combatChar.NeedUseItem.IsValid() || combatChar.GetPreparingItem().IsValid())
            {
                __result = CombatCharacterStateType.PrepareUseItem;
                return;
            }

            if (combatChar.NeedPauseJumpMove)
            {
                __result = CombatCharacterStateType.JumpMove;
                return;
            }

            if (combatChar.NeedEnterSpecialShow)
            {
                __result = CombatCharacterStateType.SpecialShow;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set command priority failed: {ex}");
        }
    }
}
