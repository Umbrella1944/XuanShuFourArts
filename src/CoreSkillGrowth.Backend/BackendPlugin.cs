using CoreSkillGrowth.Shared;
using GameData.Domains.Global;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace XuanShuFourArts.Backend;

[PluginConfig("XuanShuFourArtsBackend", "Umbrella", "1.1.0.0")]
public sealed class BackendPlugin : TaiwuRemakePlugin
{
    private Harmony _harmony;

    public override void Initialize()
    {
        ModLog.Load(ModIdStr);
        StrengthSettings.Load(ModIdStr);
        GrowthSettings.Load(ModIdStr);
        _harmony = new Harmony(GetGuid());
        _harmony.PatchAll(typeof(BackendPlugin).Assembly);

        CoreSkillGrowthConfigPatch.ApplyAll("Backend");
    }

    public override void Dispose()
    {
        _harmony?.UnpatchSelf();
    }
}

[HarmonyPatch(typeof(GlobalDomain), "ReloadAllConfigData")]
internal static class LanguageReloadPatch
{
    [HarmonyPostfix]
    private static void ReapplyCoreSkillConfig()
    {
        CoreSkillGrowthConfigPatch.ApplyAll("Backend/LanguageChange");
    }
}
