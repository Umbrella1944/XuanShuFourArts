using System;
using System.Globalization;
#if BACKEND
using GameData.Domains;
#endif

namespace CoreSkillGrowth.Shared;

// One startup snapshot per game process. Settings changes require a restart:
// config rows, cached character values and combat effects must change together.
internal static class StrengthSettings
{
    internal const int DefaultPercent = 100;
    internal const int MinPercent = 10;
    internal const int MaxPercent = 300;
    internal static readonly string[] Keys =
    {
        "ReturningFrameStrength", "ShadowStepStrength", "FormRestoringStrength",
        "BloodRiftStrength", "FourPieceStrength"
    };

    private static int[] _percent = { 100, 100, 100, 100, 100 };

    internal static int ReturningFrame => _percent[0];
    internal static int ShadowStep => _percent[1];
    internal static int FormRestoring => _percent[2];
    internal static int BloodRift => _percent[3];
    internal static int FourPiece => _percent[4];
    internal static int SetCombatBonus => Scale(15, FourPiece);
    internal static int SetNormalAttackBonus => Scale(100, FourPiece);
    internal static int SetActiveBonus => Scale(40, FourPiece);
    internal static int SetPityStep => Scale(15, FourPiece);
    internal static int BloodRiftChance => Math.Min(100, Scale(20, BloodRift));
    internal static int BloodRiftPowerPerStack => Scale(150, BloodRift);
    internal static bool NpcBloodRiftEnabled { get; private set; } = true;

    internal static void Load(string modIdStr)
    {
        int[] values = new int[Keys.Length];
        for (int i = 0; i < values.Length; i++)
        {
            int value = DefaultPercent;
            try
            {
#if BACKEND
                bool found = DomainManager.Mod.GetSetting(modIdStr, Keys[i], ref value);
#else
                bool found = ModManager.GetSetting(modIdStr, Keys[i], ref value);
#endif
                if (!found) value = DefaultPercent;
            }
            catch (Exception ex)
            {
                ModLog.Error($"[XuanShuFourArts] Setting {Keys[i]} unavailable, using 100%: {ex.Message}");
                value = DefaultPercent;
            }
            values[i] = Math.Max(MinPercent, Math.Min(MaxPercent, value));
        }
        _percent = values;
        NpcBloodRiftEnabled = ReadBool(modIdStr, "NpcBloodRiftEnabled", true);
        ModLog.Info($"[XuanShuFourArts] Startup strength (%): {string.Join(",", values)}. Changes require restart.");
    }

    private static bool ReadBool(string modIdStr, string key, bool defaultValue)
    {
        bool value = defaultValue;
        try
        {
#if BACKEND
            DomainManager.Mod.GetSetting(modIdStr, key, ref value);
#else
            ModManager.GetSetting(modIdStr, key, ref value);
#endif
        }
        catch (Exception ex)
        {
            ModLog.Error($"[XuanShuFourArts] Setting {key} unavailable, using {defaultValue}: {ex.Message}");
            value = defaultValue;
        }
        return value;
    }

    internal static int ForSkill(short skillId)
    {
        return skillId >= 0 && skillId < 4 ? _percent[skillId] : DefaultPercent;
    }

    internal static int Scale(int value, int percent)
    {
        long product = (long)value * percent;
        long rounded = product >= 0 ? (product + 50) / 100 : (product - 50) / 100;
        return (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, rounded));
    }

    internal static short ScaleShort(int value, int percent)
    {
        return (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, Scale(value, percent)));
    }

    internal static sbyte ScaleSByte(int value, int percent)
    {
        return (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, Scale(value, percent)));
    }

    internal static int Amplify(int value, int bonusPercent)
    {
        return (int)Math.Max(0L, Math.Min(int.MaxValue, (long)value * (100L + bonusPercent) / 100));
    }

    internal static short NextHealSpeed(ref int progress, bool fullSet, int returningFramePercent, int fourPiecePercent)
    {
        // At 100% this exactly preserves the old 1-in-10 / 1-in-5 schedule.
        if (returningFramePercent == 100 && fourPiecePercent == 100)
        {
            progress = (progress + 1) % 10;
            return (short)(progress % (fullSet ? 5 : 10) == 0 ? 1 : 0);
        }
        // Carry fractional progress instead of rounding the interval; high
        // settings may supply 2 native progress units in one update.
        progress += returningFramePercent * (fullSet ? 100 + fourPiecePercent : 100);
        short speed = (short)(progress / 100000);
        progress %= 100000;
        return speed;
    }

    internal static int InjuryRecoveryCount(int fractionalRoll)
    {
        return InjuryRecoveryCount(fractionalRoll, FormRestoring);
    }

    internal static int InjuryRecoveryCount(int fractionalRoll, int formRestoringPercent)
    {
        return formRestoringPercent / 100 + (fractionalRoll < formRestoringPercent % 100 ? 1 : 0);
    }

    internal static string InjuryRecoveryText => (FormRestoring / 100.0).ToString("0.##", CultureInfo.InvariantCulture);
}

internal static class StrengthDescriptions
{
    internal static string BloodRift(string language)
    {
        switch (ModLanguage.Normalize(language))
        {
            case "CN":
                return $"完整普通攻击结算后，若本轮普通攻击至少命中1次，有{StrengthSettings.BloodRiftChance}%机率获得2层血隙并触发1次血隙追击；每层血隙使血隙追击威力+{StrengthSettings.BloodRiftPowerPerStack}%，最多10层并持续整场战斗。血隙追击必中、必重创，且无法被反震；进入伤害结算时消耗全部血隙。";
            case "CNH":
                return ModLanguage.ToTraditionalChinese(BloodRift("CN"));
            case "KO":
                return $"일반 공격이 끝난 후 한 번 이상 명중하면 {StrengthSettings.BloodRiftChance}% 확률로 혈극 2중첩과 혈극 추격 1회가 발동합니다. 중첩마다 혈극 추격 위력이 {StrengthSettings.BloodRiftPowerPerStack}% 증가하며 전투 중 최대 10중첩됩니다. 혈극 추격은 반드시 명중하고 치명타가 되며 반격 피해를 받지 않습니다. 피해 계산 시 모든 혈극 중첩을 소모합니다.";
            case "JP":
                return $"通常攻撃の完了後、1回以上命中していれば{StrengthSettings.BloodRiftChance}%の確率で血隙2層を得て血隙追撃を1回発動します。血隙1層ごとに追撃威力+{StrengthSettings.BloodRiftPowerPerStack}%、戦闘中最大10層。血隙追撃は必中・必重创で反震を受けず、ダメージ確定時に全血隙を消費します。";
            default:
                return $"After a complete Normal Attack resolves, if at least one hit landed, there is a {StrengthSettings.BloodRiftChance}% chance to gain 2 Blood-Rift stacks and trigger 1 Blood-Rift Pursuit. Each stack increases Blood-Rift Pursuit power by {StrengthSettings.BloodRiftPowerPerStack}%, up to 10 stacks for the whole battle. Blood-Rift Pursuit always hits, always crits, and cannot trigger bounce damage. All Blood-Rift stacks are consumed on damage settlement.";
        }
    }

    internal static string FormRestoring(string language)
    {
        string lang = ModLanguage.Normalize(language);
        string fractionNote = "";
        if (StrengthSettings.FormRestoring % 100 != 0)
        {
            fractionNote = ModLanguage.Select(lang,
                "不足1层的部分按概率额外恢复1层。",
                " Fractional stacks are resolved as a chance to heal one additional stack.",
                " 소수 단계는 확률로 1단계를 추가 회복합니다.",
                " 端数は確率で追加1層回復します。");
        }
        string description = ModLanguage.Select(lang,
            $"此功法防御结束时，运用者身上残毁的部位的伤势平均减少{StrengthSettings.InjuryRecoveryText}层，随后，运用者全身的伤势会被随机平均分配。",
            $"At the end of this Martial Art's defense, the user reduces the injuries on their Maimed parts by an average of {StrengthSettings.InjuryRecoveryText} stack(s). Then, all injuries on the user's body are randomly and evenly redistributed.",
            $"이 무공의 방어가 끝나면 훼손 부위의 상처가 평균 {StrengthSettings.InjuryRecoveryText}단계 감소한 후 전신의 상처가 무작위로 균등 재분배됩니다.",
            $"この功法の防御終了時、損傷部位の傷を平均{StrengthSettings.InjuryRecoveryText}層減らし、その後、全身の傷をランダムに均等再分配します。");
        return description + fractionNote;
    }

    internal static string SetEffects(string language, bool detailed)
    {
        int combat = StrengthSettings.SetCombatBonus;
        int attack = StrengthSettings.SetNormalAttackBonus;
        int active = StrengthSettings.SetActiveBonus;
        int pity = StrengthSettings.SetPityStep;
        int recovery = StrengthSettings.FourPiece;
        string growthNote = !GrowthSettings.Enabled ? "" : ModLanguage.Select(language,
            "\n成长：实际强度随运用者成长（精纯、轮回、年龄、所学功法）。",
            "\nGrowth: effective strength scales with the wielder (Consummate, reincarnations, age, learned skills).",
            "\n성장: 실제 강도가 사용자의 정순, 윤회, 나이 및 습득 무공에 따라 변합니다.",
            "\n成長：実効強度は使用者の精純・輪廻・年齢・習得功法で変化します。");
        if (!detailed)
        {
            return ModLanguage.Select(language,
                $"4件：主战斗属性 +{combat}%；普通攻击威力 +{attack}%。\n按住 Alt 查看详细效果。",
                $"4pc: Major combat attributes +{combat}%; Normal Attack Power +{attack}%.\nHold Alt for details.",
                $"4종: 주요 전투 속성 +{combat}%, 일반 공격 위력 +{attack}%.\n자세한 내용은 Alt를 누르세요.",
                $"4点：主要戦闘属性+{combat}%、通常攻撃威力+{attack}%。\nAltで詳細表示。") ;
        }
        return ModLanguage.Select(language,
            $"四诀共鸣：攻击、防御、命中、化解 +{combat}%。\n万噬血隙·一线微裂：普通攻击威力 +{attack}%。\n万噬血隙·后影潮复：血隙连续4次未触发后开始获得保底加成，每次失败使触发机率+{pity}%，最高100%。\n玄息归骸·百损归枢：内功被动恢复效果 +{recovery}%。\n照影游身·逐影成双：照影游身步持续期间，施展移动速度提高效果 +{active}%。\n两仪还形·二气重垣：两仪还形功持续期间，施展提供的御体、御气、卸力、拆招、闪避效果 +{active}%。" + growthNote,
            $"Four Arts Resonance: Attack, Defense, Hit, and Deflection +{combat}%.\nBlood-Rift - Hairline Rift: Normal Attack Power +{attack}%.\nBlood-Rift - Tide of Aftershadows: After Blood-Rift fails to trigger 4 times in a row, trigger assurance begins; each further failure increases trigger chance by {pity}%, up to 100%.\nReturning-Frame - Wounds Return to Pivot: Inner Art passive recovery +{recovery}%.\nShadow Step - Twin Shadows: Shadow-Reflecting Wandering Step's active Movement Speed bonus +{active}% while active.\nTwofold Qi - Double Rampart: Yin-Yang Form-Restoring Art's active Phy. Defense, Qi Defense, Resistance, Parry, and Dodge bonuses +{active}% while active." + growthNote,
            $"사결 공명: 공격, 방어, 명중, 화해 +{combat}%.\n혈극·일선미렬: 일반 공격 위력 +{attack}%.\n혈극·후영조복: 혈극이 4회 연속 발동하지 않으면 보장이 시작되어 이후 실패마다 발동률 +{pity}%, 최대 100%.\n현식귀해·백손귀추: 내공 지속 회복 +{recovery}%.\n조영유신·축영성쌍: 조영유신보 지속 중 시전 이동 속도 +{active}%.\n양의환형·이기중원: 양의환형공 지속 중 어체, 어기, 하력, 분쇄, 회피 +{active}%." + growthNote,
            $"四诀共鳴：攻撃・防御・命中・化解+{combat}%。\n万噬血隙・一線微裂：通常攻撃威力+{attack}%。\n万噬血隙・後影潮復：血隙が4回連続で発動しなかった後、保証が開始され、以後の失敗ごとに発動率+{pity}%、最大100%。\n玄息帰骸・百損帰枢：内功の持続回復+{recovery}%。\n照影遊身・逐影成双：照影遊身歩の効果中、発動移動速度+{active}%。\n両儀還形・二気重垣：両儀還形功の効果中、御体・御気・卸力・拆招・回避+{active}%。" + growthNote);
    }
}
