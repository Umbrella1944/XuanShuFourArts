using CoreSkillGrowth.Shared;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace XuanShuFourArts.Frontend;

[PluginConfig("XuanShuFourArtsFrontend", "Umbrella", "1.0.0")]
public sealed class FrontendPlugin : TaiwuRemakePlugin
{
    private Harmony _harmony;

    public override void Initialize()
    {
        _harmony = new Harmony(GetGuid());
        _harmony.PatchAll(typeof(FrontendPlugin).Assembly);

        CoreSkillGrowthConfigPatch.ApplyAll("Frontend");
    }

    public override void Dispose()
    {
        _harmony?.UnpatchSelf();
    }
}
