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
                Console.WriteLine($"[XuanShuFourArts] Setting {Keys[i]} unavailable, using 100%: {ex.Message}");
                value = DefaultPercent;
            }
            values[i] = Math.Max(MinPercent, Math.Min(MaxPercent, value));
        }
        _percent = values;
        Console.WriteLine($"[XuanShuFourArts] Startup strength (%): {string.Join(",", values)}. Changes require restart.");
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

    internal static short NextHealSpeed(ref int progress, bool fullSet)
    {
        // At 100% this exactly preserves the old 1-in-10 / 1-in-5 schedule.
        if (ReturningFrame == 100 && FourPiece == 100)
        {
            progress = (progress + 1) % 10;
            return (short)(progress % (fullSet ? 5 : 10) == 0 ? 1 : 0);
        }
        // Carry fractional progress instead of rounding the interval; high
        // settings may supply 2 native progress units in one update.
        progress += ReturningFrame * (fullSet ? 100 + FourPiece : 100);
        short speed = (short)(progress / 100000);
        progress %= 100000;
        return speed;
    }

    internal static int InjuryRecoveryCount(int fractionalRoll)
    {
        return FormRestoring / 100 + (fractionalRoll < FormRestoring % 100 ? 1 : 0);
    }

    internal static string InjuryRecoveryText => (FormRestoring / 100.0).ToString("0.##", CultureInfo.InvariantCulture);
}

internal static class StrengthDescriptions
{
    internal static string BloodRift(bool english)
    {
        return english
            ? $"After a complete Normal Attack resolves, if at least one hit landed, there is a {StrengthSettings.BloodRiftChance}% chance to gain 2 Blood-Rift stacks and trigger 1 Blood-Rift Pursuit. Each stack increases Blood-Rift Pursuit power by {StrengthSettings.BloodRiftPowerPerStack}%, up to 10 stacks for the whole battle. Blood-Rift Pursuit always hits, always crits, and cannot trigger bounce damage. All Blood-Rift stacks are consumed on damage settlement."
            : $"完整普通攻击结算后，若本轮普通攻击至少命中1次，有{StrengthSettings.BloodRiftChance}%机率获得2层血隙并触发1次血隙追击；每层血隙使血隙追击威力+{StrengthSettings.BloodRiftPowerPerStack}%，最多10层并持续整场战斗。血隙追击必中、必重创，且无法被反震；进入伤害结算时消耗全部血隙。";
    }

    internal static string FormRestoring(bool english)
    {
        string fractionNote = StrengthSettings.FormRestoring % 100 == 0 ? "" : english
            ? " Fractional stacks are resolved as a chance to heal one additional stack."
            : "不足1层的部分按概率额外恢复1层。";
        return (english
            ? $"At the end of this Martial Art's defense, the user reduces the injuries on their Maimed parts by an average of {StrengthSettings.InjuryRecoveryText} stack(s). Then, all injuries on the user's body are randomly and evenly redistributed."
            : $"此功法防御结束时，运用者身上残毁的部位的伤势平均减少{StrengthSettings.InjuryRecoveryText}层，随后，运用者全身的伤势会被随机平均分配。") + fractionNote;
    }

    internal static string SetEffects(bool english, bool detailed)
    {
        int combat = StrengthSettings.SetCombatBonus;
        int attack = StrengthSettings.SetNormalAttackBonus;
        int active = StrengthSettings.SetActiveBonus;
        int pity = StrengthSettings.SetPityStep;
        int recovery = StrengthSettings.FourPiece;
        if (!detailed)
            return english
                ? $"4pc: Major combat attributes +{combat}%; Normal Attack Power +{attack}%.\nHold Alt for details."
                : $"4件：主战斗属性 +{combat}%；普通攻击威力 +{attack}%。\n按住 Alt 查看详细效果。";
        return english
            ? $"Four Arts Resonance: Attack, Defense, Hit, and Deflection +{combat}%.\nBlood-Rift - Hairline Rift: Normal Attack Power +{attack}%.\nBlood-Rift - Tide of Aftershadows: After Blood-Rift fails to trigger 4 times in a row, trigger assurance begins; each further failure increases trigger chance by {pity}%, up to 100%.\nReturning-Frame - Wounds Return to Pivot: Inner Art passive recovery +{recovery}%.\nShadow Step - Twin Shadows: Shadow-Reflecting Wandering Step's active Movement Speed bonus +{active}% while active.\nTwofold Qi - Double Rampart: Yin-Yang Form-Restoring Art's active Phy. Defense, Qi Defense, Resistance, Parry, and Dodge bonuses +{active}% while active."
            : $"四诀共鸣：攻击、防御、命中、化解 +{combat}%。\n万噬血隙·一线微裂：普通攻击威力 +{attack}%。\n万噬血隙·后影潮复：血隙连续4次未触发后开始获得保底加成，每次失败使触发机率+{pity}%，最高100%。\n玄息归骸·百损归枢：内功被动恢复效果 +{recovery}%。\n照影游身·逐影成双：照影游身步持续期间，施展移动速度提高效果 +{active}%。\n两仪还形·二气重垣：两仪还形功持续期间，施展提供的御体、御气、卸力、拆招、闪避效果 +{active}%。";
    }
}
