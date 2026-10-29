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
    public bool NormalAttackRollEligible;
    public bool CurrentNormalAttackHit;

    public void ClearFreeAttack()
    {
        PendingFreeAttack = false;
        FreeAttackActive = false;
        FreeAttackSnapshotStacks = 0;
        FreeAttackDamageSettled = false;
    }

    public void ResetNormalAttackRecord()
    {
        NormalAttackRollEligible = false;
        CurrentNormalAttackHit = false;
    }
}

internal static class BloodRiftPursuitRuntime
{
    private const int PityStartFailures = 4;
    private const int PityMaxChancePercent = 100;
    private const int TriggerStacks = 2;
    private const int MaxStacks = 10;

    // Growth-scaled per wielder: the slider value is the base, the wielder's
    // growth factor scales it at the moment of use.
    private static int ChanceFor(int charId)
    {
        return Math.Min(100, GrowthSettings.ScaleByFactor(StrengthSettings.BloodRiftChance, GrowthSettings.FactorForCharacter(charId)));
    }

    private static int PityStepFor(int charId)
    {
        return GrowthSettings.ScaleByFactor(StrengthSettings.SetPityStep, GrowthSettings.FactorForCharacter(charId));
    }

    private static int PursuitPowerPerStackFor(int charId)
    {
        return GrowthSettings.ScaleByFactor(StrengthSettings.BloodRiftPowerPerStack, GrowthSettings.FactorForCharacter(charId));
    }

    private static readonly Dictionary<int, BloodRiftCombatState> CombatStates = new Dictionary<int, BloodRiftCombatState>();

    internal static void ClearAll()
    {
        CombatStates.Clear();
    }

    internal static void OnNormalAttackBegin(
        DataContext context,
        CombatCharacter attacker)
    {
        try
        {
            if (attacker == null ||
                !CombatStates.TryGetValue(attacker.GetId(), out BloodRiftCombatState state) ||
                !state.PendingFreeAttack)
            {
                return;
            }

            if (!CanStartFreeAttack(attacker))
            {
                LoseStack(state);
                SyncStackDisplay(context, attacker, state);
                ShowBloodRiftTip(attacker, 1);
                state.ClearFreeAttack();
                state.ResetNormalAttackRecord();
                return;
            }

            state.PendingFreeAttack = false;
            state.FreeAttackActive = true;
            state.FreeAttackSnapshotStacks = Math.Max(state.Stacks, 0);
            state.FreeAttackDamageSettled = false;
            state.CurrentNormalAttackHit = false;
            if (state.FreeAttackSnapshotStacks <= 0)
            {
                SyncStackDisplay(context, attacker, state);
                state.ClearFreeAttack();
            }
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] Blood-Rift pursuit begin failed: {ex}");
        }
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

            if (!CombatStates.TryGetValue(attacker.GetId(), out BloodRiftCombatState state) ||
                !state.NormalAttackRollEligible ||
                state.PendingFreeAttack ||
                state.FreeAttackActive)
            {
                return;
            }

            state.CurrentNormalAttackHit = true;
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] Blood-Rift pursuit hit record failed: {ex}");
        }
    }

    internal static void OnNormalAttackPrepareEnd(DataContext context, int charId)
    {
        try
        {
            if (CombatStates.TryGetValue(charId, out BloodRiftCombatState existingState) &&
                (existingState.PendingFreeAttack || existingState.FreeAttackActive))
            {
                return;
            }

            CombatCharacter combatChar = DomainManager.Combat.GetElement_CombatCharacterDict(charId);
            if (CanCreateNormalAttackTicket(combatChar))
            {
                BloodRiftCombatState state = GetCombatState(charId);
                state.ResetNormalAttackRecord();
                state.NormalAttackRollEligible = true;
                return;
            }

            if (existingState != null)
            {
                existingState.ResetNormalAttackRecord();
            }
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] Blood-Rift pursuit record reset failed: {ex}");
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
                state.ResetNormalAttackRecord();
                return;
            }

            if (!state.NormalAttackRollEligible)
            {
                state.ResetNormalAttackRecord();
                return;
            }

            if (!state.CurrentNormalAttackHit)
            {
                state.ResetNormalAttackRecord();
                return;
            }

            state.ResetNormalAttackRecord();
            bool fullSet = FourSetActiveSkillBonus.HasFullSetEquipped(attacker.GetId());
            int chance = CalcChance(state, fullSet, attacker.GetId());
            bool triggered = RollPercent(context, chance);
            ModLog.Debug($"Blood-Rift char={attacker.GetId()} chance={chance} triggered={triggered} failures={state.FailureCount}");
            if (!triggered)
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

            if (!CanStartFreeAttack(attacker))
            {
                LoseStack(state);
                SyncStackDisplay(context, attacker, state);
                ShowBloodRiftTip(attacker, 1);
                state.ClearFreeAttack();
                return;
            }

            attacker.NormalAttackFree();
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] Blood-Rift pursuit queue failed: {ex}");
        }
    }

    internal static bool ShouldForceBloodRiftPursuitHit(CombatCharacter attacker, short skillId)
    {
        if (skillId >= 0 ||
            attacker == null ||
            attacker.GetIsFightBack() ||
            !CombatStates.TryGetValue(attacker.GetId(), out BloodRiftCombatState state))
        {
            return false;
        }

        return state.FreeAttackActive &&
            state.FreeAttackSnapshotStacks > 0;
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

    internal static bool IsBloodRiftPursuitCharacter(CombatCharacter character)
    {
        return character != null &&
            CombatStates.TryGetValue(character.GetId(), out BloodRiftCombatState state) &&
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
            !IsEligibleWielder(attacker) ||
            !FourSetActiveSkillBonus.HasFullSetEquipped(attacker.GetId()))
        {
            return false;
        }

        Character character = attacker.GetCharacter();
        return character != null &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.TaiZuChangQuan);
    }

    internal static void AmplifyNormalAttackPower(int charId, ref int power)
    {
        int bonus = GrowthSettings.ScaleByFactor(StrengthSettings.SetNormalAttackBonus, GrowthSettings.FactorForCharacter(charId));
        power = StrengthSettings.Amplify(power, bonus);
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

        int bonusPercent = state.FreeAttackSnapshotStacks * PursuitPowerPerStackFor(attacker.GetId());
        power = StrengthSettings.Amplify(power, bonusPercent);
        if (!state.FreeAttackDamageSettled)
        {
            state.Stacks = 0;
            state.FreeAttackDamageSettled = true;
            ShowBloodRiftTip(attacker, 2);
        }
    }

    private static bool IsEligibleWielder(CombatCharacter attacker)
    {
        return StrengthSettings.NpcBloodRiftEnabled ||
            attacker.GetId() == DomainManager.Taiwu.GetTaiwuCharId();
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
            !IsInNormalAttackState(attacker, CombatCharacterStateType.Attack) ||
            !IsEligibleWielder(attacker) ||
            !DomainManager.Combat.IsInCombat())
        {
            return false;
        }

        Character character = attacker.GetCharacter();
        return character != null &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.TaiZuChangQuan);
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

    private static bool CanCreateNormalAttackTicket(CombatCharacter combatChar)
    {
        if (combatChar == null ||
            !DomainManager.Combat.IsInCombat() ||
            !IsEligibleWielder(combatChar) ||
            combatChar.GetIsFightBack() ||
            !IsInNormalAttackState(combatChar, CombatCharacterStateType.PrepareAttack) ||
            combatChar.IsAutoNormalAttacking ||
            combatChar.IsAutoNormalAttackingSpecial)
        {
            return false;
        }

        Character character = combatChar.GetCharacter();
        return character != null &&
            character.IsCombatSkillEquipped(CoreSkillGrowthConfigPatch.TaiZuChangQuan);
    }

    private static bool IsInNormalAttackState(CombatCharacter combatChar, CombatCharacterStateType stateType)
    {
        // 1.1 removed IsBreakAttacking. The native preparation event is raised
        // before PrepareAttack transitions to Attack; hit events run in Attack.
        // CastSkill and UnlockAttack remain separate and cannot earn a ticket.
        return combatChar?.StateMachine?.GetCurrentState()?.StateType == stateType;
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

    private static int CalcChance(BloodRiftCombatState state, bool fullSet, int charId)
    {
        if (!fullSet)
        {
            return ChanceFor(charId);
        }

        int extra = state.FailureCount >= PityStartFailures
            ? (state.FailureCount - PityStartFailures + 1) * PityStepFor(charId)
            : 0;
        return Math.Min(ChanceFor(charId) + extra, PityMaxChancePercent);
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

[HarmonyPatch(typeof(Events), "RaiseNormalAttackBegin")]
internal static class BloodRiftNormalAttackBeginPatch
{
    [HarmonyPostfix]
    private static void StartBloodRiftFreeAttack(
        DataContext context,
        CombatCharacter attacker,
        CombatCharacter defender,
        sbyte trickType,
        int pursueIndex)
    {
        BloodRiftPursuitRuntime.OnNormalAttackBegin(context, attacker);
    }
}

[HarmonyPatch(typeof(Events), "RaiseNormalAttackPrepareEnd")]
internal static class BloodRiftNormalAttackPrepareEndPatch
{
    [HarmonyPostfix]
    private static void ResetBloodRiftNormalAttackRecord(DataContext context, int charId, bool isAlly)
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

[HarmonyPatch(typeof(CombatDomain), "AddBounceDamage", new[] { typeof(CombatContext), typeof(sbyte) })]
internal static class BloodRiftNormalAttackBouncePatch
{
    [HarmonyPrefix]
    private static bool DisableBloodRiftPursuitBounce(CombatContext context)
    {
        return !BloodRiftPursuitRuntime.IsBloodRiftPursuitNormalAttack(context);
    }
}

[HarmonyPatch(typeof(CombatDomain), "CanPursue")]
internal static class BloodRiftCanPursuePatch
{
    private static readonly FieldInfo SaveDyingEffectTriggeredField =
        AccessTools.Field(typeof(CombatDomain), "_saveDyingEffectTriggerd");

    [HarmonyPrefix]
    private static bool ForceFullBloodRiftPursuit(CombatDomain __instance, CombatCharacter character, ref bool __result)
    {
        if (!BloodRiftPursuitRuntime.IsBloodRiftPursuitCharacter(character))
        {
            return true;
        }

        try
        {
            __result = CanSafelyPursue(__instance, character);
            return false;
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] Blood-Rift pursue patch failed: {ex}");
            return true;
        }
    }

    private static bool CanSafelyPursue(CombatDomain combatDomain, CombatCharacter character)
    {
        if (combatDomain == null ||
            character == null ||
            !combatDomain.IsInCombat() ||
            !DomainManager.SpecialEffect.ModifyData(character.GetId(), -1, 252, dataValue: true))
        {
            return false;
        }

        CombatCharacter defender = combatDomain.GetCombatCharacter(!character.IsAlly, tryGetCoverCharacter: true);
        return defender != null &&
            character.PursueAttackCount < 5 &&
            !character.GetIsFightBack() &&
            !defender.GetIsFightBack() &&
            defender.ChangeCharId < 0 &&
            character.ChangeCharId < 0 &&
            !defender.NeedChangeBossPhase &&
            !combatDomain.IsCharacterFallen(defender) &&
            !combatDomain.IsCharacterFallen(character) &&
            !IsSaveDyingEffectTriggered(combatDomain) &&
            character.AttackForceHitCount <= 0 &&
            character.AttackForceMissCount <= 0;
    }

    private static bool IsSaveDyingEffectTriggered(CombatDomain combatDomain)
    {
        return SaveDyingEffectTriggeredField != null &&
            SaveDyingEffectTriggeredField.GetValue(combatDomain) is bool triggered &&
            triggered;
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
internal static class BloodRiftPursuitHitPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(CombatDomain),
            "ApplyHitOddsSpecialEffect",
            new[]
            {
                typeof(CombatCharacter),
                typeof(CombatCharacter),
                typeof(int),
                typeof(sbyte),
                typeof(short)
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

        ModLog.Warn("[XuanShuFourArts] Blood-Rift hit patch skipped: ApplyHitOddsSpecialEffect target not found.");
        return false;
    }

    [HarmonyPostfix]
    private static void ForceBloodRiftPursuitHit(CombatCharacter attacker, short skillId, ref int __result)
    {
        if (BloodRiftPursuitRuntime.ShouldForceBloodRiftPursuitHit(attacker, skillId))
        {
            // Keep the hit odds non-negative so vanilla effects that inspect hit data
            // do not enter fallback body-part paths with invalid normal-attack tricks.
            __result = Math.Max(__result, 100);
        }
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

        ModLog.Warn("[XuanShuFourArts] Blood-Rift power patch skipped: CalcAndAddInjury target not found.");
        return false;
    }

    [HarmonyPrefix]
    private static void AmplifyBloodRiftNormalAttack(CombatContext context, ref int power)
    {
        if (BloodRiftPursuitRuntime.ShouldAmplifyNormalAttack(context))
        {
            BloodRiftPursuitRuntime.AmplifyNormalAttackPower(context.Attacker.GetId(), ref power);
        }

        BloodRiftPursuitRuntime.AmplifyBloodRiftPursuitPower(context, ref power);
    }
}
