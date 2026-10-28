using System;
using System.Collections.Generic;
using System.Reflection;
using Config;
using Config.Common;
using Config.ConfigCells.Character;
using GameData.Domains.Character;

namespace CoreSkillGrowth.Shared;

internal static class CoreSkillGrowthConfigPatch
{
    internal const short PeiRanJue = 0;
    internal const short XiaoZongYueGong = 1;
    internal const short ShuiHuoYingQiGong = 2;
    internal const short TaiZuChangQuan = 3;

    // Practice-bonus property tables at 100% strength, in the same order as the
    // skill constants above. Shared with the growth patch, which injects the
    // per-wielder delta on top of the baked (growth-neutral) values.
    internal static readonly (short PropertyId, short Value)[][] CoreSkillPropertyTables =
    {
        new (short, short)[] { (17, 100), (9, 100), (15, 100), (27, 40), (26, 40), (22, 40), (19, 40), (28, 100), (29, 100), (30, 100), (31, 100), (32, 100), (33, 100) },
        new (short, short)[] { (10, 100), (8, 100), (14, 100), (25, 40), (20, 40) },
        new (short, short)[] { (16, 100), (12, 100), (13, 100), (21, 40), (23, 40) },
        new (short, short)[] { (11, 100), (6, 100), (7, 100), (18, 40), (24, 40) },
    };

    private static int ConfiguredStrength(int sliderPercent)
    {
        return sliderPercent;
    }

    private static short _peiRanDirectEffectId = -1;
    private static short _peiRanReverseEffectId = -1;
    private static short _xiaoZongDirectEffectId = -1;
    private static short _xiaoZongReverseEffectId = -1;
    private static short _shuiHuoDirectEffectId = -1;
    private static short _shuiHuoReverseEffectId = -1;
    private static short _taiZuDisplayEffectId = -1;

    private static readonly BindingFlags InstanceFieldFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    internal static short TaiZuDisplayEffectId => _taiZuDisplayEffectId;
    internal static short PeiRanEffectId => _peiRanDirectEffectId;
    internal static short XiaoZongEffectId => _xiaoZongDirectEffectId;
    internal static short ShuiHuoEffectId => _shuiHuoDirectEffectId;

    public static void ApplyAll(string sideName)
    {
        try
        {
            string language = ModLanguage.Current;
            PatchCharacterPropertyDisplay();
            RegisterCoreSkillEffects(language);
            PatchPeiRanJue(language);
            PatchXiaoZongYueGong(language);
            PatchShuiHuoYingQiGong(language);
            RegisterTaiZuDisplayEffect(language);
            PatchTaiZuChangQuan(language);
            ModLog.Info($"[XuanShuFourArts] {sideName}: effect registry patch applied. Language={language}");
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] {sideName}: failed to apply patch: {ex}");
            throw;
        }
    }

    private static void PatchCharacterPropertyDisplay()
    {
        for (short propertyId = 18; propertyId <= 27; propertyId++)
        {
            CharacterPropertyDisplayItem item =
                ((ConfigData<CharacterPropertyDisplayItem, short>)(object)CharacterPropertyDisplay.Instance).GetItem(propertyId);

            SetField(item, "IsPercent", true);
        }
    }

    private static string L(string language, string chinese, string english, string korean = null, string japanese = null)
    {
        return ModLanguage.Select(language, chinese, english, korean ?? english, japanese ?? english);
    }

    private static string[] L(string language, string[] chinese, string[] english, string[] korean = null, string[] japanese = null)
    {
        return ModLanguage.Select(language, chinese, english, korean ?? english, japanese ?? english);
    }

    private static void RegisterCoreSkillEffects(string language)
    {
        _peiRanReverseEffectId = FindOrAddEmptySpecialEffect(
            L(language, "玄息归骸经", "Profound Breath Returning-Frame Sutra", "현식귀해경", "玄息帰骸経"),
            PeiRanJue,
            L(language,
                Array.Empty<string>(),
                Array.Empty<string>()),
            L(language,
                new[] { $"此功法效果无法被封禁；运用者的新增伤势、旧时伤势标记均会以不同速度自行消除。当前恢复倍率：{StrengthSettings.ReturningFrame}%" },
                new[] { $"This art's effect cannot be sealed. The practitioner's newly gained and pre-existing Injury Marks clear on their own at different rates. Current recovery strength: {StrengthSettings.ReturningFrame}%." },
                new[] { $"이 무공의 효과는 봉인할 수 없습니다. 새 상처와 오래된 상처 표시가 각기 다른 속도로 사라집니다. 현재 회복 배율: {StrengthSettings.ReturningFrame}%" },
                new[] { $"この功法の効果は封じられません。新傷・旧傷のマークがそれぞれの速度で自然に消えます。現在の回復倍率：{StrengthSettings.ReturningFrame}%" }),
            L(language,
                new[] { $"此功法效果无法被封禁；运用者的新增伤势、旧时伤势标记均会以不同速度自行消除。当前恢复倍率：{StrengthSettings.ReturningFrame}%" },
                new[] { $"This art's effect cannot be sealed. The practitioner's newly gained and pre-existing Injury Marks clear on their own at different rates. Current recovery strength: {StrengthSettings.ReturningFrame}%." },
                new[] { $"이 무공의 효과는 봉인할 수 없습니다. 새 상처와 오래된 상처 표시가 각기 다른 속도로 사라집니다. 현재 회복 배율: {StrengthSettings.ReturningFrame}%" },
                new[] { $"この功法の効果は封じられません。新傷・旧傷のマークがそれぞれの速度で自然に消えます。現在の回復倍率：{StrengthSettings.ReturningFrame}%" }));
        _peiRanDirectEffectId = _peiRanReverseEffectId;

        _xiaoZongReverseEffectId = FindOrAddEmptySpecialEffect(
            L(language, "照影游身步", "Shadow-Reflecting Wandering Step", "조영유신보", "照影遊身歩"),
            XiaoZongYueGong,
            L(language,
                new[] { "变化所受伤害" },
                new[] { "Changes the damage taken." },
                new[] { "받는 피해 변환" },
                new[] { "被ダメージを変換" }),
            L(language,
                new[] { $"此功法生效期间：运用者受到直接伤害时，会优先消耗脚力抵消伤害；脚力不足时，剩余伤害正常结算。当前脚力抵伤效率：{StrengthSettings.ShadowStep}%" },
                new[] { $"While this Martial Art is active: Direct DMG taken by the user first consumes Stamina to offset the damage. If Stamina is insufficient, the remaining damage is resolved normally. Current Stamina-to-damage efficiency: {StrengthSettings.ShadowStep}%." },
                new[] { $"이 무공의 효과가 지속되는 동안 직접 피해를 받으면 각력을 먼저 소모하여 피해를 상쇄합니다. 각력이 부족하면 남은 피해가 그대로 적용됩니다. 현재 각력 피해 상쇄 효율: {StrengthSettings.ShadowStep}%" },
                new[] { $"この功法の効果中、直接ダメージを受けると脚力を優先消費して軽減します。脚力不足時は残りのダメージを通常計算します。現在の脚力軽減効率：{StrengthSettings.ShadowStep}%" }),
            L(language,
                new[] { $"此功法生效期间：运用者受到直接伤害时，会优先消耗脚力抵消伤害；脚力不足时，剩余伤害正常结算。当前脚力抵伤效率：{StrengthSettings.ShadowStep}%" },
                new[] { $"While this Martial Art is active: Direct DMG taken by the user first consumes Stamina to offset the damage. If Stamina is insufficient, the remaining damage is resolved normally. Current Stamina-to-damage efficiency: {StrengthSettings.ShadowStep}%." },
                new[] { $"이 무공의 효과가 지속되는 동안 직접 피해를 받으면 각력을 먼저 소모하여 피해를 상쇄합니다. 각력이 부족하면 남은 피해가 그대로 적용됩니다. 현재 각력 피해 상쇄 효율: {StrengthSettings.ShadowStep}%" },
                new[] { $"この功法の効果中、直接ダメージを受けると脚力を優先消費して軽減します。脚力不足時は残りのダメージを通常計算します。現在の脚力軽減効率：{StrengthSettings.ShadowStep}%" }));
        _xiaoZongDirectEffectId = _xiaoZongReverseEffectId;

        _shuiHuoReverseEffectId = FindOrAddEmptySpecialEffect(
            L(language, "两仪还形功", "Yin-Yang Form-Restoring Art", "양의환형공", "両儀還形功"),
            ShuiHuoYingQiGong,
            L(language,
                new[] { "伤势随机分配" },
                new[] { "Distributes injuries at random." },
                new[] { "상처 무작위 분배" },
                new[] { "傷のランダム分配" }),
            L(language,
                new[] { StrengthDescriptions.FormRestoring(language) },
                new[] { StrengthDescriptions.FormRestoring(language) },
                new[] { StrengthDescriptions.FormRestoring(language) },
                new[] { StrengthDescriptions.FormRestoring(language) }),
            L(language,
                new[] { StrengthDescriptions.FormRestoring(language) },
                new[] { StrengthDescriptions.FormRestoring(language) },
                new[] { StrengthDescriptions.FormRestoring(language) },
                new[] { StrengthDescriptions.FormRestoring(language) }));
        _shuiHuoDirectEffectId = _shuiHuoReverseEffectId;
    }

    private static void RegisterTaiZuDisplayEffect(string language)
    {
        _taiZuDisplayEffectId = FindOrAddEmptySpecialEffect(
            L(language, "万噬血隙·后影潮复", "Blood-Rift - Tide of Aftershadows", "만서혈극·후영조복", "万噬血隙・後影潮復"),
            TaiZuChangQuan,
            L(language,
                new[] { "后影潮复触发", "血隙层数流失", "血隙追击爆发" },
                new[] { "Tide of Aftershadows triggers.", "Blood-Rift stack lost.", "Blood-Rift Pursuit releases." },
                new[] { "후영조복 발동", "혈극 중첩 소실", "혈극 추격 발동" },
                new[] { "後影潮復発動", "血隙層消失", "血隙追撃発動" }),
            L(language,
                new[] { StrengthDescriptions.BloodRift(language) },
                new[] { StrengthDescriptions.BloodRift(language) },
                new[] { StrengthDescriptions.BloodRift(language) },
                new[] { StrengthDescriptions.BloodRift(language) }),
            L(language,
                new[] { StrengthDescriptions.BloodRift(language) },
                new[] { StrengthDescriptions.BloodRift(language) },
                new[] { StrengthDescriptions.BloodRift(language) },
                new[] { StrengthDescriptions.BloodRift(language) }));
    }

    private static void PatchPeiRanJue(string language)
    {
        CombatSkillItem item = GetSkill(PeiRanJue);

        SetField(item, "Name", L(language, "玄息归骸经", "Profound Breath Returning-Frame Sutra", "현식귀해경", "玄息帰骸経"));
        SetField(item, "Desc", L(language,
            "玄息归骸经言人身有枢，百损皆可循枢而返。其息入骨，不争一时盈虚，只令裂处回流、缺处复位；新痕若浮萍触水，易聚易散，旧厄若沉铁入渊，缓缓无声。外禁可压诸术，独难夺此中枢之转。",
            "It teaches that the body has an inner pivot. A hidden breath sinks into bone: fresh marks scatter like duckweed, while old afflictions descend like iron into the abyss. Outer seals may bind many arts, yet this hidden turning remains beyond their grasp.",
            "현식귀해경은 사람의 몸에 중추가 있어 모든 상처가 그 중추를 따라 돌아올 수 있음을 가르칩니다. 그 기운은 뼈에 스며들어 갈라진 곳을 메우고 새 상처는 물 위의 부평처럼 흩어지며, 오래된 상처는 심연에 가라앉은 쇠처럼 조용히 사라집니다. 외부의 봉인도 이 중추의 회전은 빼앗지 못합니다.",
            "玄息帰骸経は、人の身に枢があり、あらゆる損傷が枢を巡って還ると説く。息は骨に沁み、新たな傷は水面の浮萍のように散り、古い厄は淵に沈む鉄のように静かに消える。外からの封印も、この枢の巡りを奪うことはできない。"));
        SetField(item, "Grade", (sbyte)8);
        SetField(item, "DirectEffectID", _peiRanDirectEffectId);
        SetField(item, "ReverseEffectID", _peiRanReverseEffectId);
        int strength = ConfiguredStrength(StrengthSettings.ReturningFrame);
        SetField(item, "TotalObtainableNeili", StrengthSettings.ScaleShort(2000, strength));
        SetField(item, "ObtainedNeiliPerLoop", StrengthSettings.ScaleShort(60, strength));
        SetField(item, "GenericGrid", (sbyte)36);
        short recovery = StrengthSettings.ScaleShort(4, strength);
        SetField(item, "RecoveryOfStanceAndBreath", new OuterAndInnerShorts(recovery, recovery));
        SetField(item, "PossibleQiArtStrategyList", new List<sbyte> { 9, 15 });
        SetField(item, "ExtraNeiliAllocationProgress", new sbyte[] { 0, 0, 0, 0, StrengthSettings.ScaleSByte(12, strength) });
        SetField(item, "QiArtStrategyGenerateProbability", StrengthSettings.ScaleSByte(8, strength));
        SetField(item, "PropertyAddList", Props(strength, CoreSkillPropertyTables[0]));
        SetField(item, "FatalDamageStep", StrengthSettings.Scale(300, strength));
        SetField(item, "MindDamageStep", StrengthSettings.Scale(50, strength));
    }

    private static void PatchXiaoZongYueGong(string language)
    {
        CombatSkillItem item = GetSkill(XiaoZongYueGong);
        int strength = ConfiguredStrength(StrengthSettings.ShadowStep);

        SetField(item, "Name", L(language, "照影游身步", "Shadow-Reflecting Wandering Step", "조영유신보", "照影遊身歩"));
        SetField(item, "Desc", L(language,
            "照影游身步本出轻身纵跃之法，后演为虚实相映之术。修习者动若逐影，止若临镜，身形似在目前，又似隔水照花，危急之间常能借一线余势，转败为安。",
            "Shadow-Reflecting Wandering Step began as a light-body leaping method, then evolved into an art of mirrored substance and illusion. Its practitioner moves as if pursuing a shadow and stops as if facing a mirror; the body seems near at hand, yet like a flower reflected across water, and in moments of danger it often borrows one last thread of momentum to turn defeat into safety.",
            "조영유신보는 가벼운 몸놀림에서 시작되어 허실이 서로 비추는 무공으로 발전했습니다. 움직일 때는 그림자를 따르고 멈출 때는 거울 앞에 선 듯하며, 가까운 듯 물 너머의 꽃처럼 아득합니다. 위기에서는 남은 기세를 빌려 패세를 만회합니다.",
            "照影遊身歩は軽身の跳躍から始まり、虚実が映じ合う術へと発展した。動は影を追い、静は鏡に臨むが如し。目前にありながら水越しの花のようにも見える。危急の際にはわずかな余勢を借りて逆転を図る。"));
        SetField(item, "Grade", (sbyte)8);
        SetField(item, "DirectEffectID", _xiaoZongDirectEffectId);
        SetField(item, "ReverseEffectID", _xiaoZongReverseEffectId);
        SetField(item, "MobilityCost", (short)10);
        SetField(item, "MobilityReduceSpeed", 40);
        SetField(item, "MoveCostMobility", 40);
        SetField(item, "AddMoveSpeedOnCast", (short)0);
        SetField(item, "AddPercentMoveSpeedOnCast", StrengthSettings.ScaleShort(70, strength));
        SetField(item, "AddAvoidOnCast", new short[4]);
        SetField(item, "PropertyAddList", Props(strength, CoreSkillPropertyTables[1]));
        SetField(item, "OuterDamageSteps", Steps(strength, 0, 0, 0, 0, 0, 100, 100));
        SetField(item, "InnerDamageSteps", Steps(strength, 0, 0, 0, 0, 0, 100, 100));
    }

    private static void PatchShuiHuoYingQiGong(string language)
    {
        CombatSkillItem item = GetSkill(ShuiHuoYingQiGong);
        int strength = ConfiguredStrength(StrengthSettings.FormRestoring);

        SetField(item, "Name", L(language, "两仪还形功", "Yin-Yang Form-Restoring Art", "양의환형공", "両儀還形功"));
        SetField(item, "Desc", L(language,
            "两仪还形功以水火相济为本，动静往复，寒暑互藏。修习者以二气护持形骸，纵一时筋骨摧折、气血逆乱，亦能使败势流转周身，不令一处先绝。",
            "Yin-Yang Form-Restoring Art is founded on the mutual aid of water and fire, with motion and stillness turning back upon each other, heat and cold hidden within one another. Its practitioner uses the two currents to guard the body; even when sinew and bone are broken and blood and Qi fall into disorder, the art can send the failing momentum through the whole body, not allowing any one part to perish first.",
            "양의환형공은 수화 상제를 바탕으로 동정이 순환하고 한서가 서로를 품습니다. 두 기운으로 몸을 보호하므로 뼈가 꺾이고 기혈이 어지러워도 패세를 온몸에 돌려 한 부위가 먼저 물러지지 않게 합니다.",
            "両儀還形功は水火の相助を本とし、動静が巡り寒暑を互いに蔵す。二気で形骸を守り、筋骨が砕け気血が乱れても、敗勢を全身に巡らせて一部の崩壊を防ぐ。"));
        SetField(item, "Grade", (sbyte)8);
        SetField(item, "DirectEffectID", _shuiHuoDirectEffectId);
        SetField(item, "ReverseEffectID", _shuiHuoReverseEffectId);
        SetField(item, "PrepareTotalProgress", 1500);
        SetField(item, "BreathStanceTotalCost", (sbyte)60);
        SetField(item, "BaseInnerRatio", (sbyte)50);
        SetField(item, "AddOuterPenetrateResistOnCast", StrengthSettings.ScaleShort(150, strength));
        SetField(item, "AddInnerPenetrateResistOnCast", StrengthSettings.ScaleShort(150, strength));
        short avoid = StrengthSettings.ScaleShort(250, strength);
        SetField(item, "AddAvoidOnCast", new short[] { avoid, avoid, avoid, 0 });
        SetField(item, "FightBackDamage", (short)0);
        SetField(item, "ContinuousFrames", (short)180);
        SetField(item, "FightBackAnimation", (string)null);
        SetField(item, "FightBackParticle", (string)null);
        SetField(item, "FightBackSound", (string)null);
        SetField(item, "PropertyAddList", Props(strength, CoreSkillPropertyTables[2]));
        SetField(item, "OuterDamageSteps", Steps(strength, 200, 200, 0, 0, 0, 0, 0));
        SetField(item, "InnerDamageSteps", Steps(strength, 200, 200, 0, 0, 0, 0, 0));
    }

    private static void PatchTaiZuChangQuan(string language)
    {
        CombatSkillItem item = GetSkill(TaiZuChangQuan);
        int strength = ConfiguredStrength(StrengthSettings.BloodRift);

        SetField(item, "Name", L(language, "万噬血隙拳", "Myriad-Devouring Blood-Rift Fist", "만서혈극권", "万噬血隙拳"));
        SetField(item, "Desc", L(language,
            "万噬血隙拳本无定形，借拳为门，纳兽为枢。其法既入身中，便不问掌指，不问锋刃，只候敌躯一线微裂；裂处方生，群凶已伏血下，前势如齿未离，后影如潮复至。追之愈久，噬之愈深，终使其形乱神离，不知所避。",
            "Formless in truth, this fist is only a gate; the beast is what remains within. Hand, edge, or point; it waits for a rift in the foe. Once blood opens, hidden fangs gather; one bite has not left before the next shadow comes. The longer it hunts, the deeper it sinks.",
            "만서혈극권은 정해진 형태가 없으며 권법을 통로로 삼고 짐승의 본능을 중추로 삼습니다. 그 법이 몸에 깃들면 손가락이나 날을 가리지 않고 적의 틈을 기다립니다. 틈이 생기면 흉기가 피 아래에 숨어들고 앞의 이빨이 떠나기 전에 뒤의 그림자가 밀려옵니다. 오래 쫓을수록 더 깊이 물어뜯어 적의 형신을 어지럽힙니다.",
            "万噬血隙拳は定形を持たず、拳を門とし獣を枢とする。掌指や刃を問わず、敵のわずかな裂を待つ。裂が生じれば凶牙は血に潜み、前の勢いが去らぬうちに後影が潮のように押し寄せる。長く追うほど深く噬らみ、敵の形と神を乱す。"));
        SetField(item, "Grade", (sbyte)8);
        SetField(item, "DirectEffectID", _taiZuDisplayEffectId);
        SetField(item, "ReverseEffectID", _taiZuDisplayEffectId);
        SetField(item, "PrepareTotalProgress", 12000);
        SetField(item, "BreathStanceTotalCost", (sbyte)80);
        SetField(item, "BaseInnerRatio", (sbyte)50);
        SetField(item, "Penetrate", StrengthSettings.ScaleShort(420, strength));
        SetField(item, "DistanceAdditionWhenCast", (short)20);
        SetField(item, "TotalHit", StrengthSettings.ScaleShort(140, strength));
        SetField(item, "TrickCost", new List<NeedTrick> { new NeedTrick(6, 1), new NeedTrick(8, 1) });
        SetField(item, "PropertyAddList", Props(strength, CoreSkillPropertyTables[3]));
        SetField(item, "OuterDamageSteps", Steps(strength, 0, 0, 0, 100, 100, 0, 0));
        SetField(item, "InnerDamageSteps", Steps(strength, 0, 0, 0, 100, 100, 0, 0));
    }

    private static short FindOrAddEmptySpecialEffect(
        string name,
        short skillTemplateId,
        string[] shortDesc,
        string[] desc,
        string[] detailedDesc,
        string[] playerCastBossSkillDesc = null)
    {
        List<SpecialEffectItem> items = GetDataArray<SpecialEffectItem>(SpecialEffect.Instance);
        for (int i = 0; i < items.Count; i++)
        {
            SpecialEffectItem existing = items[i];
            if (existing != null &&
                existing.SkillTemplateId == skillTemplateId &&
                existing.Name == name)
            {
                ApplySpecialEffectText(existing, skillTemplateId, name, shortDesc, desc, detailedDesc, playerCastBossSkillDesc);
                ApplyEmptySpecialEffectBehavior(existing);
                return existing.TemplateId;
            }
        }

        short templateId = checked((short)items.Count);
        SpecialEffectItem item = new SpecialEffectItem();

        SetField(item, "TemplateId", templateId);
        ApplySpecialEffectText(item, skillTemplateId, name, shortDesc, desc, detailedDesc, playerCastBossSkillDesc);
        ApplyEmptySpecialEffectBehavior(item);
        items.Add(item);
        return templateId;
    }

    private static void ApplyEmptySpecialEffectBehavior(SpecialEffectItem item)
    {
        SetField(item, "EffectActiveType", (sbyte)-1);
        SetField(item, "MinEffectCount", (short)1);
        SetField(item, "MaxEffectCount", (short)-1);
        SetField(item, "RequireAttackPower", (sbyte)-1);
        SetField(item, "AffectRequirePower", Array.Empty<int>());
        SetField(item, "PowerDamageFactors", Array.Empty<int>());
        SetField(item, "ClassName", (string)null);
    }

    private static void ApplySpecialEffectText(
        SpecialEffectItem item,
        short skillTemplateId,
        string name,
        string[] shortDesc,
        string[] desc,
        string[] detailedDesc,
        string[] playerCastBossSkillDesc)
    {
        SetField(item, "SkillTemplateId", skillTemplateId);
        SetField(item, "Name", name);
        SetField(item, "ShortDesc", shortDesc ?? Array.Empty<string>());
        SetField(item, "Desc", desc ?? Array.Empty<string>());
        SetField(item, "DetailedDesc", detailedDesc ?? Array.Empty<string>());
        SetField(item, "PlayerCastBossSkillDesc", playerCastBossSkillDesc ?? Array.Empty<string>());
    }

    private static CombatSkillItem GetSkill(short templateId)
    {
        return ((ConfigData<CombatSkillItem, short>)(object)CombatSkill.Instance).GetItem(templateId);
    }

    private static void SetField<T>(object item, string fieldName, T value)
    {
        FieldInfo field = FindField(item.GetType(), fieldName);
        if (field == null)
        {
            throw new MissingFieldException(item.GetType().FullName, fieldName);
        }

        field.SetValue(item, value);
    }

    private static List<T> GetDataArray<T>(object config)
    {
        FieldInfo field = FindField(config.GetType(), "_dataArray");
        if (field == null)
        {
            throw new MissingFieldException(config.GetType().FullName, "_dataArray");
        }

        return (List<T>)field.GetValue(config);
    }

    private static FieldInfo FindField(Type type, string fieldName)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, InstanceFieldFlags);
            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static int[] Steps(int strength, params int[] values)
    {
        for (int i = 0; i < values.Length; i++) values[i] = StrengthSettings.Scale(values[i], strength);
        return values;
    }

    private static List<PropertyAndValue> Props(int strength, (short propertyId, short value)[] values)
    {
        List<PropertyAndValue> result = new List<PropertyAndValue>(values.Length);
        foreach ((short propertyId, short value) in values)
        {
            result.Add(new PropertyAndValue(propertyId, StrengthSettings.ScaleShort(value, strength)));
        }

        return result;
    }
}
