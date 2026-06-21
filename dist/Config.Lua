return {
    Title = "Four Arts of the Mystic Pivot",
    Source = 0,
    FileId = 3747599301,
    Version = "1.0.0.8",
    GameVersion = "1.0.13",
    BackendPlugins = {
        [1] = "XuanShuFourArtsB.dll",
    },
    FrontendPlugins = {
        [1] = "XuanShuFourArtsF.dll",
    },
    Author = "Umbrella",
    Description = [=[Four Arts of the Mystic Pivot

Author: Umbrella
Game: The Scroll of Taiwu
Mod Version: 1.0.0.8
Supported Game Version: 1.0.13

1. Overview

Four Arts of the Mystic Pivot reforges four basic sectless Tier 9 Martial Arts into a Divine Tier 1 core set:
Chant of Abundance, Minute Leap, Yin-Yang Iron Skin, and Emperor Long Fist.

Their original low learning and usage requirements are kept, but their names, descriptions, Practice Effects, combat effects, Orthodox / Heterodox effects, and display logic are reworked.

The goal is not to replace the whole vanilla Martial Arts system. These four arts are designed as long-term Practice Effect cores, combat support tools, and build foundations.

Inspired by:
https://steamcommunity.com/sharedfiles/filedetails/?id=3746556210

2. 1.0.0.8 Set Bonus Update

Added the Four Arts set bonus system.

When all four arts are equipped for Practice, the set bonus provides:
- Major combat attributes +20%.
- Normal Attack power +100%.
- Profound Breath Returning-Frame Sutra's Inner Art passive recovery is strengthened.
- Myriad-Devouring Blood-Rift Fist's Blood-Rift chain is strengthened.
- Shadow-Reflecting Wandering Step's active Movement Speed effect is doubled.
- Yin-Yang Form-Restoring Art's active defensive bonuses are doubled.

Added an in-game set bonus tooltip. Hold Alt to view detailed set bonus information.
Reduced the four arts' secondary Practice Effect attributes to the 70% tier.

3. Four Arts

Profound Breath Returning-Frame Sutra
Original Art: Chant of Abundance
Type: Qi Arts
Role: survival, Injury Marker clearing, long-term Practice Effect core

Core effect cannot be Sealed. The practitioner's newly gained and pre-existing Injury Markers clear on their own at different rates.

Shadow-Reflecting Wandering Step
Original Art: Minute Leap
Type: Footwork Arts
Role: mobility, survival, Direct DMG-to-Stamina conversion

While active, it increases Movement Speed, restores Stamina, and converts all Direct DMG taken into Stamina loss. The four-piece set bonus doubles this art's active Movement Speed effect.

Yin-Yang Form-Restoring Art
Original Art: Yin-Yang Iron Skin
Type: Unique Arts / Protection
Role: short defense window, injury redistribution

While active, it provides Phy. Defense, Qi Defense, Resistance, Parry, and Dodge. The four-piece set bonus doubles these active defensive bonuses. At the end of its defense, it reduces injuries on Maimed parts and randomly redistributes all injuries on the user's body.

Myriad-Devouring Blood-Rift Fist
Original Art: Emperor Long Fist
Type: Fist Arts / Annihilation
Role: follow-up chain, passive Direct DMG core

Core effect cannot be Sealed. Whenever the practitioner inflicts a Direct Injury Marker, they immediately launch an additional attack with reduced Accuracy accumulation. With the four-piece set bonus, chain damage increases by 150% to 250% until the combo is interrupted.

4. Notes

This mod conflicts with other mods that change Chant of Abundance, Minute Leap, Yin-Yang Iron Skin, or Emperor Long Fist.

It may also conflict with mods that change related Martial Art data tables, effect tables, or display logic.

Load this mod after similar Martial Art mods if needed. Actual results depend on in-game load order.

After changing the game language, please fully exit and restart the game.

This mod mainly overrides Martial Art data when the game loads. It should be safe to add or remove mid-save in theory, but backing up your save is still recommended.

This is a strong core-build mod and is not intended to preserve vanilla balance.

If you encounter error messages, display issues, effect conflicts, or version compatibility problems, please leave feedback.]=],
    Cover = "xuanshu_sijue_character_cover2_1024.jpg",
    WorkshopCover = "xuanshu_sijue_character_cover2_1024.jpg",
    TagList = {
        [1] = "Modifications",
        [2] = "Compatible Mods",
    },
    Visibility = 0,
    DefaultSettings = { },
    SettingGroups = {
        [1] = "Default",
    },
    ChangeConfig = false,
    HasArchive = false,
    NeedRestartWhenSettingChanged = true,
    DetailImageList = {
        [1] = "xuanshu_sijue_character_cover2_1024.jpg",
    },
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
    },
}
