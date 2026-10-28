using CoreSkillGrowth.Shared;
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
    }

    public override void Dispose()
    {
        _harmony?.UnpatchSelf();
        FourSetNewTooltipCombatSkillPatch.Dispose();
        FourSetMouseTipPanelPatch.ResetCache();
        FourSetCombatViewState.Reset();
        FourSetUiState.Reset();
    }
}
