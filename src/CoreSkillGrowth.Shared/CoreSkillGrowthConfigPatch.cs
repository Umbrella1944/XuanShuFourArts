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
            bool useEnglish = IsEnglishLanguage();
            PatchCharacterPropertyDisplay();
            RegisterCoreSkillEffects(useEnglish);
            PatchPeiRanJue(useEnglish);
            PatchXiaoZongYueGong(useEnglish);
            PatchShuiHuoYingQiGong(useEnglish);
            RegisterTaiZuDisplayEffect(useEnglish);
            PatchTaiZuChangQuan(useEnglish);
            Console.WriteLine($"[XuanShuFourArts] {sideName}: effect registry patch applied. Language={(useEnglish ? "EN" : "CN")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] {sideName}: failed to apply patch: {ex}");
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

    private static bool IsEnglishLanguage()
    {
        string languageKey = GetCurrentLanguageKey();
        return string.Equals(languageKey, "EN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(languageKey, "English", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetCurrentLanguageKey()
    {
        try
        {
            return LocalStringManager.CurLanguageKey;
        }
        catch
        {
        }

        try
        {
            Type helperType = Type.GetType("GameDataExtensions.LocalStringManagerHelper, Assembly-CSharp");
            PropertyInfo property = helperType?.GetProperty("CurLanguageKey", BindingFlags.Public | BindingFlags.Static);
            return property?.GetValue(null) as string;
        }
        catch
        {
            return null;
        }
    }

    private static string L(bool useEnglish, string chinese, string english)
    {
        return useEnglish ? english : chinese;
    }

    private static string[] L(bool useEnglish, string[] chinese, string[] english)
    {
        return useEnglish ? english : chinese;
    }

    private static void RegisterCoreSkillEffects(bool useEnglish)
    {
        _peiRanReverseEffectId = FindOrAddEmptySpecialEffect(
            L(useEnglish, "玄息归骸经", "Profound Breath Returning-Frame Sutra"),
            PeiRanJue,
            L(useEnglish,
                Array.Empty<string>(),
                Array.Empty<string>()),
            L(useEnglish,
                new[]
                {
                    $"此功法效果无法被封禁；运用者的新增伤势、旧时伤势标记均会以不同速度自行消除。当前恢复倍率：{StrengthSettings.ReturningFrame}%"
                },
                new[]
                {
                    $"This art's effect cannot be sealed. The practitioner's newly gained and pre-existing Injury Marks clear on their own at different rates. Current recovery strength: {StrengthSettings.ReturningFrame}%."
                }),
            L(useEnglish,
                new[]
                {
                    $"此功法效果无法被封禁；运用者的新增伤势、旧时伤势标记均会以不同速度自行消除。当前恢复倍率：{StrengthSettings.ReturningFrame}%"
                },
                new[]
                {
                    $"This art's effect cannot be sealed. The practitioner's newly gained and pre-existing Injury Marks clear on their own at different rates. Current recovery strength: {StrengthSettings.ReturningFrame}%."
                }));
        _peiRanDirectEffectId = _peiRanReverseEffectId;

        _xiaoZongReverseEffectId = FindOrAddEmptySpecialEffect(
            L(useEnglish, "照影游身步", "Shadow-Reflecting Wandering Step"),
            XiaoZongYueGong,
            L(useEnglish,
                new[] { "变化所受伤害" },
                new[] { "Changes the damage taken." }),
            L(useEnglish,
                new[] { $"此功法生效期间：运用者受到直接伤害时，会优先消耗脚力抵消伤害；脚力不足时，剩余伤害正常结算。当前脚力抵伤效率：{StrengthSettings.ShadowStep}%" },
                new[] { $"While this Martial Art is active: Direct DMG taken by the user first consumes Stamina to offset the damage. If Stamina is insufficient, the remaining damage is resolved normally. Current Stamina-to-damage efficiency: {StrengthSettings.ShadowStep}%." }),
            L(useEnglish,
                new[] { $"此功法生效期间：运用者受到直接伤害时，会优先消耗脚力抵消伤害；脚力不足时，剩余伤害正常结算。当前脚力抵伤效率：{StrengthSettings.ShadowStep}%" },
                new[] { $"While this Martial Art is active: Direct DMG taken by the user first consumes Stamina to offset the damage. If Stamina is insufficient, the remaining damage is resolved normally. Current Stamina-to-damage efficiency: {StrengthSettings.ShadowStep}%." }));
        _xiaoZongDirectEffectId = _xiaoZongReverseEffectId;

        _shuiHuoReverseEffectId = FindOrAddEmptySpecialEffect(
            L(useEnglish, "两仪还形功", "Yin-Yang Form-Restoring Art"),
            ShuiHuoYingQiGong,
            L(useEnglish,
                new[] { "伤势随机分配" },
                new[] { "Distributes injuries at random." }),
            L(useEnglish,
                new[] { StrengthDescriptions.FormRestoring(false) },
                new[] { StrengthDescriptions.FormRestoring(true) }),
            L(useEnglish,
                new[] { StrengthDescriptions.FormRestoring(false) },
                new[] { StrengthDescriptions.FormRestoring(true) }));
        _shuiHuoDirectEffectId = _shuiHuoReverseEffectId;
    }

    private static void RegisterTaiZuDisplayEffect(bool useEnglish)
    {
        _taiZuDisplayEffectId = FindOrAddEmptySpecialEffect(
            L(useEnglish, "万噬血隙·后影潮复", "Blood-Rift - Tide of Aftershadows"),
            TaiZuChangQuan,
            L(useEnglish,
                new[] { "后影潮复触发", "血隙层数流失", "血隙追击爆发" },
                new[] { "Tide of Aftershadows triggers.", "Blood-Rift stack lost.", "Blood-Rift Pursuit releases." }),
            L(useEnglish,
                new[] { StrengthDescriptions.BloodRift(false) },
                new[] { StrengthDescriptions.BloodRift(true) }),
            L(useEnglish,
                new[] { StrengthDescriptions.BloodRift(false) },
                new[] { StrengthDescriptions.BloodRift(true) }));
    }

    private static void PatchPeiRanJue(bool useEnglish)
    {
        CombatSkillItem item = GetSkill(PeiRanJue);

        SetField(item, "Name", L(useEnglish, "玄息归骸经", "Profound Breath Returning-Frame Sutra"));
        SetField(item, "Desc", L(useEnglish,
            "玄息归骸经言人身有枢，百损皆可循枢而返。其息入骨，不争一时盈虚，只令裂处回流、缺处复位；新痕若浮萍触水，易聚易散，旧厄若沉铁入渊，缓缓无声。外禁可压诸术，独难夺此中枢之转。",
            "It teaches that the body has an inner pivot. A hidden breath sinks into bone: fresh marks scatter like duckweed, while old afflictions descend like iron into the abyss. Outer seals may bind many arts, yet this hidden turning remains beyond their grasp."));
        SetField(item, "Grade", (sbyte)8);
        SetField(item, "DirectEffectID", _peiRanDirectEffectId);
        SetField(item, "ReverseEffectID", _peiRanReverseEffectId);
        int strength = StrengthSettings.ReturningFrame;
        SetField(item, "TotalObtainableNeili", StrengthSettings.ScaleShort(2000, strength));
        SetField(item, "ObtainedNeiliPerLoop", StrengthSettings.ScaleShort(60, strength));
        SetField(item, "GenericGrid", (sbyte)36);
        short recovery = StrengthSettings.ScaleShort(4, strength);
        SetField(item, "RecoveryOfStanceAndBreath", new OuterAndInnerShorts(recovery, recovery));
        SetField(item, "PossibleQiArtStrategyList", new List<sbyte> { 9, 15 });
        SetField(item, "ExtraNeiliAllocationProgress", new sbyte[] { 0, 0, 0, 0, StrengthSettings.ScaleSByte(12, strength) });
        SetField(item, "QiArtStrategyGenerateProbability", StrengthSettings.ScaleSByte(8, strength));
        SetField(item, "PropertyAddList", Props(strength,
            (17, 100),
            (9, 100),
            (15, 100),
            (27, 40),
            (26, 40),
            (22, 40),
            (19, 40),
            (28, 100),
            (29, 100),
            (30, 100),
            (31, 100),
            (32, 100),
            (33, 100)));
        SetField(item, "FatalDamageStep", StrengthSettings.Scale(300, strength));
        SetField(item, "MindDamageStep", StrengthSettings.Scale(50, strength));
    }

    private static void PatchXiaoZongYueGong(bool useEnglish)
    {
        CombatSkillItem item = GetSkill(XiaoZongYueGong);
        int strength = StrengthSettings.ShadowStep;

        SetField(item, "Name", L(useEnglish, "照影游身步", "Shadow-Reflecting Wandering Step"));
        SetField(item, "Desc", L(useEnglish,
            "照影游身步本出轻身纵跃之法，后演为虚实相映之术。修习者动若逐影，止若临镜，身形似在目前，又似隔水照花，危急之间常能借一线余势，转败为安。",
            "Shadow-Reflecting Wandering Step began as a light-body leaping method, then evolved into an art of mirrored substance and illusion. Its practitioner moves as if pursuing a shadow and stops as if facing a mirror; the body seems near at hand, yet like a flower reflected across water, and in moments of danger it often borrows one last thread of momentum to turn defeat into safety."));
        SetField(item, "Grade", (sbyte)8);
        SetField(item, "DirectEffectID", _xiaoZongDirectEffectId);
        SetField(item, "ReverseEffectID", _xiaoZongReverseEffectId);
        SetField(item, "MobilityCost", (short)10);
        SetField(item, "MobilityReduceSpeed", 40);
        SetField(item, "MoveCostMobility", 40);
        SetField(item, "AddMoveSpeedOnCast", (short)0);
        SetField(item, "AddPercentMoveSpeedOnCast", StrengthSettings.ScaleShort(70, strength));
        SetField(item, "AddAvoidOnCast", new short[4]);
        SetField(item, "PropertyAddList", Props(strength,
            (10, 100),
            (8, 100),
            (14, 100),
            (25, 40),
            (20, 40)));
        SetField(item, "OuterDamageSteps", Steps(strength, 0, 0, 0, 0, 0, 100, 100));
        SetField(item, "InnerDamageSteps", Steps(strength, 0, 0, 0, 0, 0, 100, 100));
    }

    private static void PatchShuiHuoYingQiGong(bool useEnglish)
    {
        CombatSkillItem item = GetSkill(ShuiHuoYingQiGong);
        int strength = StrengthSettings.FormRestoring;

        SetField(item, "Name", L(useEnglish, "两仪还形功", "Yin-Yang Form-Restoring Art"));
        SetField(item, "Desc", L(useEnglish,
            "两仪还形功以水火相济为本，动静往复，寒暑互藏。修习者以二气护持形骸，纵一时筋骨摧折、气血逆乱，亦能使败势流转周身，不令一处先绝。",
            "Yin-Yang Form-Restoring Art is founded on the mutual aid of water and fire, with motion and stillness turning back upon each other, heat and cold hidden within one another. Its practitioner uses the two currents to guard the body; even when sinew and bone are broken and blood and Qi fall into disorder, the art can send the failing momentum through the whole body, not allowing any one part to perish first."));
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
        SetField(item, "PropertyAddList", Props(strength,
            (16, 100),
            (12, 100),
            (13, 100),
            (21, 40),
            (23, 40)));
        SetField(item, "OuterDamageSteps", Steps(strength, 200, 200, 0, 0, 0, 0, 0));
        SetField(item, "InnerDamageSteps", Steps(strength, 200, 200, 0, 0, 0, 0, 0));
    }

    private static void PatchTaiZuChangQuan(bool useEnglish)
    {
        CombatSkillItem item = GetSkill(TaiZuChangQuan);
        int strength = StrengthSettings.BloodRift;

        SetField(item, "Name", L(useEnglish, "万噬血隙拳", "Myriad-Devouring Blood-Rift Fist"));
        SetField(item, "Desc", L(useEnglish,
            "万噬血隙拳本无定形，借拳为门，纳兽为枢。其法既入身中，便不问掌指，不问锋刃，只候敌躯一线微裂；裂处方生，群凶已伏血下，前势如齿未离，后影如潮复至。追之愈久，噬之愈深，终使其形乱神离，不知所避。",
            "Formless in truth, this fist is only a gate; the beast is what remains within. Hand, edge, or point; it waits for a rift in the foe. Once blood opens, hidden fangs gather; one bite has not left before the next shadow comes. The longer it hunts, the deeper it sinks."));
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
        SetField(item, "PropertyAddList", Props(strength,
            (11, 100),
            (6, 100),
            (7, 100),
            (18, 40),
            (24, 40)));
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

    private static List<PropertyAndValue> Props(int strength, params (short propertyId, short value)[] values)
    {
        List<PropertyAndValue> result = new List<PropertyAndValue>(values.Length);
        foreach ((short propertyId, short value) in values)
        {
            result.Add(new PropertyAndValue(propertyId, StrengthSettings.ScaleShort(value, strength)));
        }

        return result;
    }
}
