using CoreSkillGrowth.Shared;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace XuanShuFourArts.Backend;

[PluginConfig("XuanShuFourArtsBackend", "Umbrella", "1.0.1.8")]
public sealed class BackendPlugin : TaiwuRemakePlugin
{
    private Harmony _harmony;

    public override void Initialize()
    {
        _harmony = new Harmony(GetGuid());
        _harmony.PatchAll(typeof(BackendPlugin).Assembly);

        CoreSkillGrowthConfigPatch.ApplyAll("Backend");
    }

    public override void Dispose()
    {
        _harmony?.UnpatchSelf();
    }
}
