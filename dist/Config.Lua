return {
    Title = "Four Arts of the Mystic Pivot",
    Source = 0,
    FileId = 3747599301,
    Version = "1.0.1.0",
    GameVersion = "1.0.24",
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
Mod Version: 1.0.1.0
Supported Game Version: 1.0.24

1. Overview

Four Arts of the Mystic Pivot reforges four basic sectless Tier 9 Martial Arts into a Divine Tier 1 core set:
Chant of Abundance, Minute Leap, Yin-Yang Iron Skin, and Emperor Long Fist.

Their original low learning and usage requirements are kept, but their names, descriptions, Practice Effects, combat effects, Orthodox / Heterodox effects, and display logic are reworked.

The goal is not to replace the whole vanilla Martial Arts system. These four arts are designed as long-term Practice Effect cores, combat support tools, and build foundations.

Inspired by:
https://steamcommunity.com/sharedfiles/filedetails/?id=3746556210

2. 1.0.1.0 Set Bonus Update

Added the Four Arts set bonus system.

When all four arts are equipped for Practice, the set bonus provides:
- Major combat attributes +15%.
- Normal Attack power +100%.
- Myriad-Devouring Blood-Rift Fist now uses an independent backend passive instead of a vanilla SpecialEffect shell; Normal Attack hits have a 20% chance to gain 2 Blood-Rift stacks and trigger 1 forced-hit, guaranteed-critical Blood-Rift Pursuit.
- Each Blood-Rift stack increases pursuit power by 200%, up to 10 stacks for the battle. Blood-Rift Pursuit consumes all stacks on damage settlement, does not trigger bounce damage, and loses 1 stack if interrupted. The four-piece set gains bad-luck protection after 6 failed hit rolls.
- Player skill, defense, footwork, item, and other explicit commands take priority over Normal Attacks while the four-piece set is active.
- Profound Breath Returning-Frame Sutra's Inner Art passive recovery is improved from 1 active tick per 10 auto-heal updates to 1 active tick per 7 auto-heal updates.
- Shadow-Reflecting Wandering Step's active Movement Speed effect +40%.
- Yin-Yang Form-Restoring Art's active defensive bonuses +40%.

Added an in-game set bonus tooltip. Hold Alt to view detailed set bonus information.
Reduced the four arts' secondary Practice Effect attributes to the 30% tier.

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

While active, it increases Movement Speed. Direct DMG taken first consumes Stamina to offset the damage; if Stamina is insufficient, the remaining damage is resolved normally. The four-piece set bonus increases this art's active Movement Speed effect by 40%.

Yin-Yang Form-Restoring Art
Original Art: Yin-Yang Iron Skin
Type: Unique Arts / Protection
Role: short defense window, injury redistribution

While active, it provides Phy. Defense, Qi Defense, Resistance, Parry, and Dodge. The four-piece set bonus increases these active defensive bonuses by 40%. At the end of its defense, it reduces injuries on Maimed parts and randomly redistributes all injuries on the user's body.

Myriad-Devouring Blood-Rift Fist
Original Art: Emperor Long Fist
Type: Fist Arts / Annihilation
Role: hidden follow-up attack, passive pressure core

This passive is resolved by the mod's independent backend rather than a vanilla SpecialEffect shell. When the practitioner's Normal Attack hits, there is a 20% chance to gain 2 Blood-Rift stacks and trigger 1 forced-hit, guaranteed-critical Blood-Rift Pursuit. Each stack increases pursuit power by 200%, up to 10 stacks for the battle. Blood-Rift Pursuit consumes all stacks on damage settlement, does not trigger bounce damage, and loses 1 stack if interrupted. Normal Attacks still trigger bounce damage normally. The four-piece set also grants Normal Attack power +100% and bad-luck protection after 6 failed hit rolls.

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
    },
}
