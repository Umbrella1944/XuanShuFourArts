return {
    Title = "Four Arts of the Mystic Pivot 1.0.3",
    Source = 0,
    FileId = 3747599301,
    Version = "1.0.3.0",
    GameVersion = "1.1.23",
    BackendPlugins = {
        [1] = "XuanShuFourArtsB.dll",
    },
    FrontendPlugins = {
        [1] = "XuanShuFourArtsF.dll",
    },
    Author = "Umbrella",
    Description = "Four Arts of the Mystic Pivot\r\n\r\nAuthor: Umbrella\r\nGame: The Scroll of Taiwu\r\nMod Version: 1.0.3.0\r\nSupported Game Version: 1.1.23.\r\n\r\n[h1]1. Overview[/h1]\r\nThis mod reforges Chant of Abundance, Minute Leap, Yin-Yang Iron Skin, and Emperor Long Fist into a Divine Tier 1 core four-piece set, allowing these basic martial arts, which were originally only suitable for early-game transition, to continue serving as core components in the late game.\r\nThe design goal of Four Arts of the Mystic Pivot is to make the four arts function as long-term Practice Effect cores, supported by a unique set bonus system.\r\n\r\n[h2]References and Thanks[/h2]\r\n\r\nSome ideas in this mod were inspired by the following mod. Thanks to the original author and the community resources for reference:\r\nhttps://steamcommunity.com/sharedfiles/filedetails/?id=3746556210\r\n\r\n[h1]2. Update List[/h1]\r\n[h2]If you do not see the update, please resubscribe and restart the game!!![/h2]\r\n[h2]If you do not see the update, please resubscribe and restart the game!!![/h2]\r\n[h2]If you do not see the update, please resubscribe and restart the game!!![/h2]\r\n\r\n[h2]Save Safety Note[/h2]\r\n My principle is to avoid damaging saves and to prevent red-text errors as much as possible.\r\nBecause of this, I chose very cautious implementation methods, which caused many difficulties. But these issues have now been resolved. This was much harder than I expected.\r\nEven so, I still cannot guarantee that errors will never happen. Thank you for your understanding. Please report problems to me in time.\r\n\r\nA small contribution of inspiration to the community. The mod is now open source.\r\nSource Code:\r\nhttps://github.com/Umbrella1944/XuanShuFourArts\r\n\r\n[h1]1.0.3.0 Compatibility and Strength Settings Update[/h1]\r\n\r\n* Updated supported game version to 1.1.23.\r\n* Updated four-art effect configuration, Blood-Rift combat checks, and equipment-screen integration for the current game version.\r\n\r\n* Added five independent strength settings: one for each art and one for the four-piece set bonus.\r\n* Each setting ranges from 10% to 300%, in steps of 10%. The default 100% preserves the previous balance.\r\n* Settings adjust the corresponding attributes, recovery, damage conversion, Blood-Rift effects, and set bonuses.\r\n* Updated Chinese and English tooltips to display the active values.\r\n\r\n[h2]1.0.0.1-1.0.1.9 Updates[/h2]\r\nPlease see the change notes for details.\r\n\r\n[h1]3. Detailed Introduction[/h1]\r\n[h2]Overall Design[/h2]\r\nThe core idea of Four Arts of the Mystic Pivot is to reshape four basic martial arts from early-game transition tools into long-term build foundations.\r\nThe overall adjustment direction is as follows:\r\n[list]\r\n[*]All four arts are reforged into Divine Tier 1 martial arts.\r\n[*]Their original Tier 9 learning and usage requirements are retained.\r\n[*]The Orthodox and Heterodox effects of the four arts are kept identical.\r\n[*]The names, descriptions, Practice Effect attributes, activation effects, combat effects, and some display logic of the four arts are adjusted.\r\n[*]Related Body and Mind Strength values have been enhanced so the four arts better fit their Divine Tier 1 positioning.\r\n[*]Existing and relatively stable in-game combat effects are prioritized. Safe custom paths have now been implemented.\r\n[/list]\r\n\r\n[h2]1. Profound Breath Returning-Frame Sutra[/h2]\r\nOriginal Art: Chant of Abundance\r\nType: Inner Art\r\nRole: Internal Breath, survival, injury-fading core Practice Effect\r\n\r\n[h2]2. Shadow-Reflecting Wandering Step[/h2]\r\nOriginal Art: Minute Leap\r\nType: Footwork\r\nRole: mobility, survival, damage-conversion core footwork\r\n\r\n[h2]3. Yin-Yang Form-Restoring Art[/h2]\r\nOriginal Art: Yin-Yang Iron Skin\r\nType: Protection Unique Art\r\nRole: short defensive window, injury redistribution protection art\r\n\r\n[h2]4. Myriad-Devouring Blood-Rift Fist[/h2]\r\nOriginal Art: Emperor Long Fist\r\nType: Fist and Palm Annihilation Art\r\nRole: gap-devouring pursuit, momentum-building annihilation core\r\nCan be triggered by all weapons / Normal Attacks.\r\n\r\n[h2]Notes[/h2]\r\nThis mod will definitely conflict with other mods that change Chant of Abundance, Minute Leap, Yin-Yang Iron Skin, or Emperor Long Fist.\r\nIt may also conflict with mods that change related martial art data tables, martial art effect tables, or attribute display logic.\r\nIt is recommended to load this mod after similar martial art modification mods. Actual results depend on the in-game effect.\r\nThis mod is equivalent to a safe outer layer. In theory, it should be safe to add or remove mid-save without red-text errors.\r\nThe structure of this mod is kept as clean as possible, and the code logic is kept as safe as possible. In theory, red-text errors should be unlikely.\r\nIf you encounter red-text errors, display issues, effect overrides, or version compatibility problems, please leave feedback.\r\n\r\nThank you all for your support.\r\n\r\nI also made a prerequisite framework mod, required for the NPC activity mod I plan to make later. If you are interested, you can try it.\r\nTaiwu Interaction Framework\r\nhttps://steamcommunity.com/sharedfiles/filedetails/?id=3749605687\r\n\r\nTaiwu Save Repair\r\nhttps://steamcommunity.com/sharedfiles/filedetails/?id=3751322965\r\n\r\nTaiwu English Localization Fixes\r\nhttps://steamcommunity.com/sharedfiles/filedetails/?id=3750166478\r\n",
    Cover = "xuanshu_sijue_character_cover2_1024.jpg",
    WorkshopCover = "xuanshu_sijue_character_cover2_1024.jpg",
    TagList = {
        [1] = "Modifications",
        [2] = "Compatible Mods",
    },
    Visibility = 0,
    DefaultSettings = {
        [1] = {
            SettingType = "Slider",
            Key = "ReturningFrameStrength",
            DisplayName = "玄息归骸经强度 / Returning-Frame (%)",
            Description = "100 为原强度。调整运功属性、内力收益、恢复和体魄神识收益；恢复另受套装加成影响。重启游戏后生效。\n100 preserves the original strength. Scales Practice bonuses, Inner Qi gains, recovery and Body/Mind bonuses. Set recovery stacks with this setting. Restart required.",
            GroupName = "强度 / Strength",
            DefaultValue = 100,
            MinValue = 10,
            MaxValue = 300,
            StepSize = 10,
        },
        [2] = {
            SettingType = "Slider",
            Key = "ShadowStepStrength",
            DisplayName = "照影游身步强度 / Shadow Step (%)",
            Description = "100 为原强度。调整运功属性、施展移速、体魄收益及每份脚力抵消伤害的效率。重启游戏后生效。\n100 preserves the original strength. Scales Practice bonuses, active Movement Speed, Body bonuses and damage absorbed per unit of Stamina. Restart required.",
            GroupName = "强度 / Strength",
            DefaultValue = 100,
            MinValue = 10,
            MaxValue = 300,
            StepSize = 10,
        },
        [3] = {
            SettingType = "Slider",
            Key = "FormRestoringStrength",
            DisplayName = "两仪还形功强度 / Form-Restoring (%)",
            Description = "100 为原强度。调整运功属性、施展防御、体魄收益和残毁部位恢复层数；不足一层按概率恢复，伤势重分配规则保留。重启游戏后生效。\n100 preserves the original strength. Scales Practice, active defenses, Body bonuses and injury recovery. Fractional recovery uses probability; redistribution is preserved. Restart required.",
            GroupName = "强度 / Strength",
            DefaultValue = 100,
            MinValue = 10,
            MaxValue = 300,
            StepSize = 10,
        },
        [4] = {
            SettingType = "Slider",
            Key = "BloodRiftStrength",
            DisplayName = "万噬血隙拳强度 / Blood-Rift (%)",
            Description = "100 为原强度。调整运功属性、施展命中与穿透、体魄收益、血隙基础触发率及每层追击威力。层数与必中必重创规则保留。重启游戏后生效。\n100 preserves the original strength. Scales Practice, Hit, Penetration, Body bonuses, base proc chance and pursuit power per stack. Stack rules and guaranteed hits/crits are preserved. Restart required.",
            GroupName = "强度 / Strength",
            DefaultValue = 100,
            MinValue = 10,
            MaxValue = 300,
            StepSize = 10,
        },
        [5] = {
            SettingType = "Slider",
            Key = "FourPieceStrength",
            DisplayName = "四件套加成强度 / Four-piece bonus (%)",
            Description = "100 为原加成。调整套装属性、普攻威力、恢复加速、施展增益及血隙保底增幅；仍需装备四诀。与单诀倍率共同作用，并非最终伤害倍率。重启游戏后生效。\n100 preserves the original bonus. Scales set attributes, Normal Attack power, recovery, active bonuses and pity increments. Requires all four arts; combines with individual strength. Not a final damage multiplier. Restart required.",
            GroupName = "强度 / Strength",
            DefaultValue = 100,
            MinValue = 10,
            MaxValue = 300,
            StepSize = 10,
        },
    },
    SettingGroups = {
        [1] = "强度 / Strength",
    },
    ChangeConfig = false,
    HasArchive = false,
    NeedRestartWhenSettingChanged = true,
    DetailImageList = { },
    UpdateLogList = {
        [1] = {
            Timestamp = 1781849819,
        },
        [2] = {
            Timestamp = 1781849910,
        },
        [3] = {
            Timestamp = 1781850636,
        },
        [4] = {
            Timestamp = 1781855833,
        },
        [5] = {
            Timestamp = 1781857797,
        },
        [6] = {
            Timestamp = 1781862602,
        },
        [7] = {
            Timestamp = 1781895555,
        },
        [8] = {
            Timestamp = 1781900989,
        },
        [9] = {
            Timestamp = 1781902334,
        },
        [10] = {
            Timestamp = 1781938120,
        },
        [11] = {
            Timestamp = 1782956378,
        },
    },
}
