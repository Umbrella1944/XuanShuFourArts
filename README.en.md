# Four Arts of the Mystic Pivot

[中文](README.md) | [English](README.en.md) | [한국어](README.ko.md)

Source project for the Steam Workshop mod **Four Arts of the Mystic Pivot** for **The Scroll of Taiwu**.

This repository contains the mod source code and release metadata. It is not affiliated with or endorsed by the developers or publishers of The Scroll of Taiwu.

Chinese architecture notes for mod authors: [docs/ARCHITECTURE.zh-CN.md](docs/ARCHITECTURE.zh-CN.md).

## Current Release

- Mod version: `1.1.0.0`
- Supported game version: `1.1.23`
- Workshop FileId: `3747599301`
- Author: `Umbrella`

Compatibility validation and remaining in-game checks: [1.1.23 maintenance report](docs/MAINTENANCE-1.1.23.zh-CN.md). Local deployment and offline checks are complete. The author published the Workshop update on 2026-09-29; actual gameplay validation remains pending.

## Features

- Reworks four base sectless Tier 9 martial arts into Divine Tier 1 core arts.
- Keeps low-tier learning, usage, and breakthrough burden for these four arts.
- Adds a four-piece set bonus when all four arts are equipped for Practice.
- Uses independent empty effect shells plus backend patches for the four core passives, avoiding changes to the original vanilla effect entries.
- Reworks Myriad-Devouring Blood-Rift Fist into an independent backend passive: a complete Normal Attack sequence that lands at least once can gain Blood-Rift stacks and trigger a forced-hit, guaranteed-critical pursuit that does not trigger bounce damage.
- Adds in-game set tooltip support with Alt details, while suppressing the large set tooltip during combat.
- Supports CN/CNH/EN/KO in-game text and runtime language switching, with Chinese fallback for unknown languages.
- Adds five independent strength sliders (10%–300%, step 10%): four individual arts and the four-piece bonus. All default to 100%; restart after changes.
- Adds per-character growth using the same algorithm for Taiwu and NPCs, with configurable Consummate, reincarnation, lifespan-relative age and learned-skill weights.
- Adds an NPC Blood-Rift and Normal Attack bonus toggle, enabled by default, and an optional rate-limited debug logging toggle.

## Strength Settings

Open this mod's detailed settings in the game's mod manager. Set each art and the set bonus independently; 100% preserves the previous balance. The sliders adjust numerical benefits, not exact final damage. Costs, requirements, durations, stack limits and core effect rules are retained. Changes take effect after a full restart, and tooltips display the active values.

For example, Blood-Rift at 50% has a 10% base trigger chance and +75% pursuit power per stack. Keeping the set slider at 100% retains the set's +100% Normal Attack power bonus. Fractional injury recovery is resolved probabilistically; other integer bonuses round to the nearest integer.

Scope and verification: [strength settings notes](docs/STRENGTH-SETTINGS.zh-CN.md). Existing player settings are preserved during local deployment; an older empty settings file uses the new 100% defaults.

## Repository Layout

```text
src/                         C# projects and source files
tools/                       Build, verification and release scripts
docs/                        Development and maintenance notes
deploy/                      Release metadata, default settings template and assets
.tmpbuild/                   Local Debug/Release outputs, ignored by Git
.tmpdeploy/                  Release-candidate staging, ignored by Git
.decompiled/                 Local API snapshots, never published
```

Historical archives are stored outside the repository and are never overwritten.

## Build Requirements

- The Scroll of Taiwu installed locally.
- .NET SDK 8.0 or newer.
- Windows is recommended, because the project references local Taiwu assemblies.

By default, the project looks for the game at:

```text
C:\Program Files (x86)\Steam\steamapps\common\The Scroll Of Taiwu
```

If your game is installed elsewhere, pass `GameDir` explicitly:

```powershell
dotnet build src/CoreSkillGrowth.Backend/CoreSkillGrowth.Backend.csproj -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\The Scroll Of Taiwu"
dotnet build src/CoreSkillGrowth.Frontend/CoreSkillGrowth.Frontend.csproj -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\The Scroll Of Taiwu"
```

Or set the environment variable `TAIWU_GAME_DIR` before building.

Build outputs are written to `.tmpbuild/<Configuration>/Plugins/`. Debug outputs are for offline checks only and must not be deployed to the game.

## Release Workflow

1. Update `deploy/Config.Lua`:
   - top-level `Version`
   - top-level `GameVersion`
   - version fields inside the preserved workshop description text
2. Preserve workshop-facing metadata:
   - Read the current English Steam Workshop page before editing `Title` or `Description`.
   - Keep the default workshop metadata in English.
   - Do not replace the default English `Title` or `Description` with the Simplified Chinese page text.
3. Keep `DetailImageList = { }` unless the workshop detail-image order intentionally needs to be replaced.
4. Before uploading, confirm the local game/workshop `Config.Lua` upload source still has the default English `Title` and `Description`; the in-game manager can rewrite it to Chinese after an upload, and uploading from that file will overwrite the Steam English page.
5. Write Steam Change Notes as `[h1]Title[/h1]` followed by `*` bullet lines; do not use `[list]` or `[*]`.
6. Build both projects.
7. Assemble the candidate in `.tmpdeploy` from Release outputs and `deploy` resources. Include the default `deploy/Settings.Lua` template, but never a player's modified local settings.

8. Upload through the in-game mod manager only after the candidate passes content, structure and SHA-256 checks.

## Local Development and Online Directories

- This working copy is the only source for changes, diffs and commits.
- `SteamLibrary\steamapps\workshop\content\838350` is for pulling published mods only, not test deployment, source maintenance or candidate generation.
- Deploy local tests to `The Scroll Of Taiwu\Mod\XuanShuFourArts` after building; never treat that directory as source.
- Do not enable the local build and the Workshop subscription at the same time.
- Historical ZIP files are stored in `E:\Work\VibeCoding\TaiWuDev\Archives` and are never overwritten.

## Compatibility Notes

- This mod patches the four base sectless martial arts: Chant of Abundance, Minute Leap, Yin-Yang Iron Skin, and Emperor Long Fist.
- It may conflict with other mods that modify these martial arts, related combat-skill data tables, effect tables, or display logic.
- Breakthrough compatibility uses minimum safeguards and should not override stronger success-rate changes from other mods.
- Skill configuration is reapplied automatically after a language change; only mod setting changes require a full restart.

## License

The source code is licensed under the MIT License. See [LICENSE](LICENSE).

Image assets and third-party game assets are not covered by the MIT License unless explicitly stated. See [ASSET_LICENSE.md](ASSET_LICENSE.md).
