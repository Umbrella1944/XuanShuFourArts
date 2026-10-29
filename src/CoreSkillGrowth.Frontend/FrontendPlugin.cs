using CoreSkillGrowth.Shared;
using FrameWork;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace XuanShuFourArts.Frontend;

[PluginConfig("XuanShuFourArtsFrontend", "Umbrella", "1.1.0.0")]
public sealed class FrontendPlugin : TaiwuRemakePlugin
{
    private Harmony _harmony;

    public override void Initialize()
    {
        ModLog.Load(ModIdStr);
        StrengthSettings.Load(ModIdStr);
        GrowthSettings.Load(ModIdStr);
        _harmony = new Harmony(GetGuid());
        _harmony.PatchAll(typeof(FrontendPlugin).Assembly);

        CoreSkillGrowthConfigPatch.ApplyAll("Frontend");
        GrowthLocalization.Apply(ModIdStr);
        GEvent.Add(UiEvents.OnLanguageChange, OnLanguageChange);
    }

    private void OnLanguageChange(ArgumentBox argBox)
    {
        CoreSkillGrowthConfigPatch.ApplyAll("Frontend/LanguageChange");
        GrowthLocalization.Apply(ModIdStr);
        FourSetUiState.Reset();
        FourSetMouseTipPanelPatch.ResetCache();
        FourSetNewTooltipCombatSkillPatch.HideFloatingHint();
    }

    public override void Dispose()
    {
        GEvent.Remove(UiEvents.OnLanguageChange, OnLanguageChange);
        _harmony?.UnpatchSelf();
        FourSetNewTooltipCombatSkillPatch.Dispose();
        FourSetMouseTipPanelPatch.ResetCache();
        FourSetCombatViewState.Reset();
        FourSetUiState.Reset();
    }
}
