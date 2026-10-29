using System;
using System.Collections.Generic;
using System.Reflection;
using CoreSkillGrowth.Shared;
using FrameWork.ModSystem;
using Game.Views.Mod;
using HarmonyLib;

namespace XuanShuFourArts.Frontend;

internal static class GrowthLocalization
{
    private static string _targetModId;

    internal static void Apply(string modIdStr)
    {
        if (ModManager.LocalMods.TryGetValue(modIdStr, out ModInfoWithDisplayData modInfo))
        {
            _targetModId = modInfo.ModId.ToString();
            ApplyToModInfo(modInfo);
        }
    }

    internal static void ApplyToModInfo(ModInfoWithDisplayData modInfo)
    {
        try
        {
            if (modInfo == null || modInfo.ModSettingEntries == null || modInfo.ModId.ToString() != _targetModId)
            {
                return;
            }

            string language = ModLanguage.Current;
            string strengthGroup = ModLanguage.SelectCurrent("强度", "Strength", "강도", "強度");
            string growthGroup = ModLanguage.SelectCurrent("成长", "Growth", "성장", "成長");
            string debugGroup = ModLanguage.SelectCurrent("调试", "Debug", "디버그", "デバッグ");
            foreach (SettingEntry entry in modInfo.ModSettingEntries)
            {
                if (entry == null || !GrowthSettingTexts.TryGet(entry.Key, language, out string displayName, out string description))
                {
                    continue;
                }

                entry.DisplayName = displayName;
                entry.Description = description;
                bool isGrowthGroup = entry.Key != null && entry.Key.StartsWith("Growth", StringComparison.Ordinal);
                bool isDebugGroup = entry.Key == "DebugLoggingEnabled";
                entry.GroupName = isDebugGroup ? debugGroup : isGrowthGroup ? growthGroup : strengthGroup;
            }

            if (modInfo.ModSettingGroups != null)
            {
                for (int i = 0; i < modInfo.ModSettingGroups.Count; i++)
                {
                    string group = modInfo.ModSettingGroups[i];
                    if (group == "强度 / Strength" || group == "强度" || group == "Strength")
                    {
                        modInfo.ModSettingGroups[i] = strengthGroup;
                    }
                    else if (group == "成长 / Growth" || group == "成长" || group == "Growth")
                    {
                        modInfo.ModSettingGroups[i] = growthGroup;
                    }
                    else if (group == "调试 / Debug" || group == "调试" || group == "Debug")
                    {
                        modInfo.ModSettingGroups[i] = debugGroup;
                    }
                }
            }

            modInfo.Title = ModLanguage.SelectCurrent("玄枢四诀", "Four Arts of the Mystic Pivot", "현추사결", "Four Arts of the Mystic Pivot");
            modInfo.Description = ModLanguage.SelectCurrent(GrowthModDescription.Chinese, modInfo.Description, modInfo.Description, modInfo.Description);
            string firstName = modInfo.ModSettingEntries.Count > 0 ? modInfo.ModSettingEntries[0].DisplayName : string.Empty;
            ModLog.Info($"[XuanShuFourArts] settings localization applied. Language={language} Entries={modInfo.ModSettingEntries.Count} First={firstName}");
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] growth localization failed: {ex}");
        }
    }
}

internal static class GrowthModDescription
{
    internal const string Chinese = @"玄枢四诀

作者：Umbrella
游戏：《太吾绘卷》
MOD 版本：1.1.0.0
支持游戏版本：1.1.23

[h1]1. 概述[/h1]
本 MOD 将沛然诀、小纵跃功、水火硬气功和太祖长拳改造为神一品核心四件套，让这些原本仅适合前期过渡的基础功法，在游戏后期仍能作为核心构筑使用。
玄枢四诀的设计目标是让四诀成为长期使用的运功效果核心，并辅以独特的套装加成系统。

[h2]参考与致谢[/h2]
本 MOD 的部分设计灵感来自以下 MOD，感谢原作者及社区提供的参考资料：
https://steamcommunity.com/sharedfiles/filedetails/?id=3746556210

[h1]2. 更新列表[/h1]
[h2]如果没有看到更新，请重新订阅并重启游戏！[/h2]
[h2]如果没有看到更新，请重新订阅并重启游戏！[/h2]
[h2]如果没有看到更新，请重新订阅并重启游戏！[/h2]

[h2]存档安全说明[/h2]
我的原则是尽量避免损坏存档和出现红字错误。为此，我选择了较为谨慎的实现方式，也因此遇到了不少困难，但这些问题目前已经解决。这项工作比预期更复杂。
即便如此，我仍无法保证错误永远不会发生。感谢你的理解，如遇到问题请及时反馈。

感谢社区提供的一点灵感。本 MOD 现已开源。
源代码：
https://github.com/Umbrella1944/XuanShuFourArts

[h1]1.1.0.0 角色成长与 NPC 效果更新[/h1]
* 新增可配置的角色成长系统，成长来源包括精纯、轮回、年龄与寿命比例、已学功法。
* 太吾与 NPC 使用同一套功法效果和成长算法，并提供 NPC 血隙与普通攻击加成开关。
* 成长在各使用者的运行时消费点分别计算，不会用太吾的成长倍率改写共享配置。
* 新增可选的限流诊断日志，并修复脚力伤害转换丢失非整数伤害余数的问题。

[h1]1.0.3.0 兼容性与强度设置更新[/h1]
* 支持游戏版本 1.1.23。
* 更新四诀效果配置、血隙战斗判定和装备界面集成，以适配当前游戏版本。
* 新增五项独立强度设置：四门单诀各一项，四件套加成为一项。
* 每项设置范围为 10% 至 300%，步进为 10%，默认 100% 保持原有平衡。
* 设置调整对应属性、恢复、伤害转换、血隙效果和套装加成。
* 中英文提示会显示当前生效数值。

[h2]1.0.0.1-1.0.1.9 更新[/h2]
详情请参阅更新日志。

[h1]3. 详细介绍[/h1]
[h2]整体设计[/h2]
核心思路是将前期过渡用的四门基础功法重塑为长期构筑基础。
主要调整如下：
[list]
[*]四门功法全部重塑为神一品功法。
[*]保留原本的九品学习与使用要求。
[*]四门功法的正练与逆练效果保持一致。
[*]调整四门功法的名称、描述、运功属性、施展效果、战斗效果及部分显示逻辑。
[*]提高相关体魄与神识收益，使其符合神一品定位。
[*]优先采用现有且稳定的游戏内战斗效果，现已实现安全的自定义路径。
[/list]

[h2]1. 玄息归骸经[/h2]
原功法：沛然诀
类型：内功
定位：内息、生存、旧伤消退核心运功效果

[h2]2. 照影游身步[/h2]
原功法：小纵跃功
类型：身法
定位：移动、生存、伤害转换核心身法

[h2]3. 两仪还形功[/h2]
原功法：水火硬气功
类型：护体绝技
定位：短时防御窗口、伤势重分配护体绝技

[h2]4. 万噬血隙拳[/h2]
原功法：太祖长拳
类型：拳掌摧破
定位：噬隙追击、蓄势爆发核心摧破
可由所有武器及普通攻击触发。

[h2]注意事项[/h2]
本 MOD 必然会与修改沛然诀、小纵跃功、水火硬气功或太祖长拳的其他 MOD 冲突。
修改相关功法表、特效表、战斗结算或提示界面的 MOD，也可能存在兼容问题。
建议将本 MOD 排在同类功法修改 MOD 之后加载，实际结果以游戏内效果为准。
本 MOD 相当于一层安全的外层实现。理论上，在存档中途添加或移除也不应引发红字错误。
实现结构尽量保持简洁，代码逻辑尽量保持安全，因此理论上不太可能出现红字错误。
但仍无法完全避免红字错误、显示问题、效果覆盖或版本兼容问题。如遇上述情况，请留言反馈。

感谢你的支持。

我还制作了一个前置框架 MOD，供计划开发的 NPC 活动 MOD 使用，欢迎尝试：
Taiwu Interaction Framework
https://steamcommunity.com/sharedfiles/filedetails/?id=3749605687

Taiwu Save Repair
https://steamcommunity.com/sharedfiles/filedetails/?id=3751322965

Taiwu English Localization Fixes
https://steamcommunity.com/sharedfiles/filedetails/?id=3750166478
";
}

[HarmonyPatch(typeof(ViewModInfo), "Refresh")]
internal static class GrowthModInfoViewPatch
{
    private static readonly FieldInfo DataField = AccessTools.Field(typeof(ViewModInfo), "_data");

    [HarmonyPrefix]
    private static void LocalizeCurrentDisplayData(ViewModInfo __instance)
    {
        GrowthLocalization.ApplyToModInfo(DataField?.GetValue(__instance) as ModInfoWithDisplayData);
    }
}

internal static class GrowthSettingTexts
{
    private static readonly Dictionary<string, (string CnName, string CnDesc, string EnName, string EnDesc, string KoName, string KoDesc, string JpName, string JpDesc)> Texts =
        new Dictionary<string, (string, string, string, string, string, string, string, string)>
    {
        ["ReturningFrameStrength"] = (
            "玄息归骸经强度", "100 为基准强度。调整运功属性、内力收益、恢复和体魄神识收益；恢复另受套装加成影响。重启生效。",
            "Returning-Frame Strength", "100 is the baseline strength. Scales Practice bonuses, Inner Qi gains, recovery and Body/Mind bonuses. Restart required.",
            "현식귀해경 강도", "100이 기준 강도입니다. 운공 속성, 내기 획득, 회복 및 체격·정신 효과를 조정하며, 회복은 세트 보너스도 적용됩니다. 재시작 후 적용됩니다.",
            "玄息帰骸経の強度", "100が基準強度です。運功属性、内気収益、回復、体魄・神識収益を調整します。回復にはセットボーナスも影響します。再起動後に有効です。"),
        ["ShadowStepStrength"] = (
            "照影游身步强度", "100 为基准强度。调整运功属性、施展移速、体魄收益及每份脚力抵消伤害的效率。重启生效。",
            "Shadow Step Strength", "100 is the baseline strength. Scales Practice bonuses, active Movement Speed, Body bonuses and damage absorbed per unit of Stamina. Restart required.",
            "조영유신보 강도", "100이 기준 강도입니다. 운공 속성, 시전 이동 속도, 체격 효과 및 각력으로 피해를 상쇄하는 효율을 조정합니다. 재시작 후 적용됩니다.",
            "照影遊身歩の強度", "100が基準強度です。運功属性、発動時の移動速度、体魄収益、脚力によるダメージ軽減効率を調整します。再起動後に有効です。"),
        ["FormRestoringStrength"] = (
            "两仪还形功强度", "100 为基准强度。调整运功属性、施展防御、体魄收益和残毁部位恢复层数；不足一层按概率恢复，伤势重分配规则保留。重启生效。",
            "Form-Restoring Strength", "100 is the baseline strength. Scales Practice, active defenses, Body bonuses and injury recovery. Fractional recovery uses probability; redistribution is preserved. Restart required.",
            "양의환형공 강도", "100이 기준 강도입니다. 운공 속성, 시전 방어, 체격 효과 및 훼손 부위 회복 단계를 조정합니다. 소수 단계는 확률로 회복하며 상처 재분배 규칙은 유지됩니다. 재시작 후 적용됩니다.",
            "両儀還形功の強度", "100が基準強度です。運功属性、発動防御、体魄収益、損傷部位の回復層数を調整します。端数は確率で回復し、傷の再分配ルールは維持されます。再起動後に有効です。"),
        ["BloodRiftStrength"] = (
            "万噬血隙拳强度", "100 为基准强度。调整运功属性、施展命中与穿透、体魄收益、血隙基础触发率及每层追击威力。层数与必中必重创规则保留。重启生效。",
            "Blood-Rift Strength", "100 is the baseline strength. Scales Practice, Hit, Penetration, Body bonuses, base proc chance and pursuit power per stack. Stack rules and guaranteed hits/crits are preserved. Restart required.",
            "만서혈극권 강도", "100이 기준 강도입니다. 운공 속성, 시전 명중·관통, 체격 효과, 혈극 기본 발동률 및 추격 위력을 조정합니다. 중첩과 필중·필중상 규칙은 유지됩니다. 재시작 후 적용됩니다.",
            "万噬血隙拳の強度", "100が基準強度です。運功属性、発動命中・貫通、体魄収益、血隙の基本発動率、追撃威力を調整します。スタックと必中・必重创のルールは維持されます。再起動後に有効です。"),
        ["FourPieceStrength"] = (
            "四件套加成强度", "100 为基准加成。调整套装属性、普攻威力、恢复加速、施展增益及血隙保底增幅；仍需装备四诀。与单诀倍率共同作用，并非最终伤害倍率。重启生效。",
            "Four-piece Bonus Strength", "100 is the baseline bonus. Scales set attributes, Normal Attack power, recovery, active bonuses and pity increments. Requires all four arts; not a final damage multiplier. Restart required.",
            "4종 세트 보너스 강도", "100이 기준 보너스입니다. 세트 속성, 일반 공격 위력, 회복 가속, 시전 보너스 및 혈극 보장 증가를 조정합니다. 네 결을 모두 장비해야 하며, 단일 결 배율과 함께 적용되지만 최종 피해 배율은 아닙니다. 재시작 후 적용됩니다.",
            "4点セットボーナスの強度", "100が基準ボーナスです。セット属性、通常攻撃威力、回復加速、発動強化、血隙の保証増幅を調整します。四诀の装備が必要で、単独倍率と併用しますが最終ダメージ倍率ではありません。再起動後に有効です。"),
        ["GrowthEnabled"] = (
            "成长模式", "开启后，四诀强度受精纯、轮回、年龄和已学功法影响；关闭时直接使用「强度」页数值。实际强度＝「强度」页数值×成长倍率；成长倍率＝基础倍率＋精纯加成＋轮回加成＋年龄加成＋功法加成。重启生效。",
            "Growth Mode", "When enabled, the four arts are affected by Consummate, reincarnations, age and learned combat skills; when disabled, they use the values on the Strength page. Effective strength = Strength page value x growth multiplier. Growth multiplier = base multiplier + Consummate bonus + reincarnation bonus + age bonus + learned skill bonus. Restart required.",
            "성장 모드", "켜면 네 결의 강도가 정순, 윤회, 나이 및 습득 무공의 영향을 받습니다. 끄면 「강도」 페이지 값을 사용합니다. 실제 강도＝「강도」 페이지 값×성장 배율이며, 성장 배율＝기본 배율＋정순 보너스＋윤회 보너스＋나이 보너스＋무공 보너스입니다. 재시작 후 적용됩니다.",
            "成長モード", "有効時、四诀の強度は精純・輪廻・年齢・習得功法の影響を受けます。無効時は「強度」ページの値を使います。実効強度＝「強度」ページの値×成長倍率。成長倍率＝基本倍率＋精純＋輪廻＋年齢＋功法の各ボーナス。再起動後に有効です。"),
        ["GrowthBasePercent"] = (
            "成长·基础倍率", "没有精纯、轮回、年龄和功法加成时的成长倍率。重启生效。",
            "Growth · Base multiplier", "Growth multiplier before bonuses from Consummate, reincarnations, age and learned skills. Restart required.",
            "성장·기본 배율", "정순, 윤회, 나이 및 무공 보너스가 없을 때의 성장 배율입니다. 재시작 후 적용됩니다.",
            "成長・基本倍率", "精純・輪廻・年齢・功法の各ボーナスを含まない成長倍率です。再起動後に有効です。"),
        ["GrowthJingChunPer"] = (
            "成长·每点精纯", "每点精纯增加的成长倍率。设为 0 可忽略精纯。重启生效。",
            "Growth · Per Consummate", "Growth multiplier added per Consummate level. Set to 0 to ignore Consummate. Restart required.",
            "성장·정순 1점당", "정순 1점당 증가하는 성장 배율입니다. 0으로 설정하면 정순을 무시합니다. 재시작 후 적용됩니다.",
            "成長・精純1点ごと", "精純1点ごとに増える成長倍率です。0にすると精純を無視します。再起動後に有効です。"),
        ["GrowthReincPer"] = (
            "成长·每世轮回", "每世轮回增加的成长倍率，仅对太吾生效。设为 0 可忽略轮回。重启生效。",
            "Growth · Per reincarnation", "Growth multiplier added per reincarnation (Taiwu only). Set to 0 to ignore reincarnations. Restart required.",
            "성장·윤회 1세당", "윤회 1세당 증가하는 성장 배율이며 태오에게만 적용됩니다. 0으로 설정하면 윤회를 무시합니다. 재시작 후 적용됩니다.",
            "成長・輪廻1世ごと", "輪廻1世ごとに増える成長倍率で、太吾にのみ適用されます。0にすると輪廻を無視します。再起動後に有効です。"),
        ["GrowthAgePerFiveYears"] = (
            "成长·每 5 岁", "年龄加成按寿命比例平滑变化，在寿命中点达到峰值，随后逐渐衰减。设为 0 可忽略年龄。重启生效。",
            "Growth · Per 5 years", "Age bonus follows a smooth lifespan-relative curve, peaking at half the character's lifespan before declining. Set to 0 to ignore age. Restart required.",
            "성장·5세마다", "나이 보너스는 수명 비율에 따라 부드럽게 변하며 수명의 중점에서 최고점에 도달한 후 점차 감소합니다. 0으로 설정하면 나이를 무시합니다. 재시작 후 적용됩니다.",
            "成長・5歳ごと", "年齢ボーナスは寿命に対する割合で滑らかに変化し、寿命の中点で最大となった後、徐々に減衰します。0にすると年齢を無視します。再起動後に有効です。"),
        ["GrowthSkillPer"] = (
            "成长·每门功法", "每门已学功法增加的成长倍率。设为 0 可忽略功法。重启生效。",
            "Growth · Per learned skill", "Growth multiplier added per learned combat skill. Set to 0 to ignore skills. Restart required.",
            "성장·무공 1개당", "습득 무공 1개당 증가하는 성장 배율입니다. 0으로 설정하면 무공을 무시합니다. 재시작 후 적용됩니다.",
            "成長・功法1つごと", "習得功法1つごとに増える成長倍率です。0にすると功法を無視します。再起動後に有効です。"),
        ["GrowthSkillCap"] = (
            "成长·功法计入上限", "最多计入多少门已学功法。设为 0 可忽略功法。重启生效。",
            "Growth · Learned skill cap", "Maximum number of learned combat skills counted. Set to 0 to ignore skills. Restart required.",
            "성장·무공 반영 상한", "성장에 반영할 습득 무공의 최대 개수입니다. 0으로 설정하면 무공을 무시합니다. 재시작 후 적용됩니다.",
            "成長・功法計上上限", "成長に算入する習得功法の最大数です。0にすると功法を無視します。再起動後に有効です。"),
        ["NpcBloodRiftEnabled"] = (
            "NPC 血隙与普攻加成", "开启后，NPC 装备四诀时也能触发血隙、进行血隙追击，并获得套装普攻威力加成；关闭时，这些效果仅对太吾生效。重启生效。",
            "NPC Blood-Rift and Normal Attack bonuses", "When enabled, NPCs with the four arts equipped can trigger Blood-Rift, perform Blood-Rift Pursuit and receive the set's Normal Attack power bonus. When disabled, these effects apply only to Taiwu. Restart required.",
            "NPC 혈극 및 일반 공격 보너스", "켜면 네 결을 장비한 NPC도 혈극 발동, 혈극 추격 및 세트 일반 공격 위력 보너스를 받습니다. 끄면 이 효과는 태오에게만 적용됩니다. 재시작 후 적용됩니다.",
            "NPCの血隙・通常攻撃ボーナス", "有効時、四诀を装備したNPCも血隙の発動・追撃とセット通常攻撃ボーナスを得ます。無効時は太吾にのみ適用されます。再起動後に有効です。"),
        ["DebugLoggingEnabled"] = (
            "调试日志", "记录详细的成长计算和血隙状态变化，便于排查问题；日常游玩建议关闭。重启生效。",
            "Debug logging", "Logs detailed growth calculations and Blood-Rift state changes for troubleshooting. Recommended off during normal play. Restart required.",
            "디버그 로그", "성장 계산과 혈극 상태 변화를 자세히 기록하여 문제 해결을 돕습니다. 일반 플레이에서는 끄는 것을 권장합니다. 재시작 후 적용됩니다.",
            "デバッグログ", "成長計算と血隙の状態変化を詳細に記録します。通常のプレイでは無効を推奨します。再起動後に有効です。"),
    };

    internal static bool TryGet(string key, string language, out string displayName, out string description)
    {
        displayName = null;
        description = null;
        if (!Texts.TryGetValue(key, out var text))
        {
            return false;
        }

        switch (ModLanguage.Normalize(language))
        {
            case "CN":
            case "CNH":
                displayName = text.CnName;
                description = text.CnDesc;
                break;
            case "KO":
                displayName = text.KoName;
                description = text.KoDesc;
                break;
            case "JP":
                displayName = text.JpName;
                description = text.JpDesc;
                break;
            default:
                displayName = text.EnName;
                description = text.EnDesc;
                break;
        }
        return true;
    }
}
