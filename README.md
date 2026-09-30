# Four Arts of the Mystic Pivot

Source project for the Steam Workshop mod **Four Arts of the Mystic Pivot** for **The Scroll of Taiwu**.

This repository contains the mod source code and release metadata. It is not affiliated with or endorsed by the developers or publishers of The Scroll of Taiwu.

Chinese architecture notes for mod authors: [docs/ARCHITECTURE.zh-CN.md](docs/ARCHITECTURE.zh-CN.md).

## Current Release

- Mod version: `1.0.3.0`
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
- Supports Chinese and English in-game text at game startup.
- Adds five independent strength sliders (10%–300%, step 10%): four individual arts and the four-piece bonus. All default to 100%; restart after changes.

## Strength Settings

Open this mod's detailed settings in the game's mod manager. Set each art and the set bonus independently; 100% preserves the previous balance. The sliders adjust numerical benefits, not exact final damage. Costs, requirements, durations, stack limits and core effect rules are retained. Changes take effect after a full restart, and tooltips display the active values.

For example, Blood-Rift at 50% has a 10% base trigger chance and +75% pursuit power per stack. Keeping the set slider at 100% retains the set's +100% Normal Attack power bonus. Fractional injury recovery is resolved probabilistically; other integer bonuses round to the nearest integer.

Scope and verification: [strength settings notes](docs/STRENGTH-SETTINGS.zh-CN.md). Existing player settings are preserved during local deployment; an older empty settings file uses the new 100% defaults.

## Repository Layout

```text
src/
  CoreSkillGrowth.Backend/    Backend Harmony patches and combat logic
  CoreSkillGrowth.Frontend/   Frontend UI tooltip patches
  CoreSkillGrowth.Shared/     Shared combat-skill config patching
dist/
  Config.Lua                  Taiwu mod metadata used by the uploader
  Settings.Lua                Default mod settings
  Plugins/                    Build output folder
  *.jpg                       Cover and workshop image assets
```

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
dotnet build src/CoreSkillGrowth.Backend/CoreSkillGrowth.Backend.csproj -p:GameDir="D:\SteamLibrary\steamapps\common\The Scroll Of Taiwu"
dotnet build src/CoreSkillGrowth.Frontend/CoreSkillGrowth.Frontend.csproj -p:GameDir="D:\SteamLibrary\steamapps\common\The Scroll Of Taiwu"
```

Or set the environment variable `TAIWU_GAME_DIR` before building.

Build outputs are written to:

```text
dist/Plugins/
```

## Release Workflow

1. Update `dist/Config.Lua`:
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
7. Sync `dist/Config.Lua`, `dist/Settings.Lua`, `dist/Plugins/`, and required image assets to:

```text
C:\Program Files (x86)\Steam\steamapps\common\The Scroll Of Taiwu\Mod\XuanShuFourArts
```

8. Upload through the in-game mod manager.

## Compatibility Notes

- This mod patches the four base sectless martial arts: Chant of Abundance, Minute Leap, Yin-Yang Iron Skin, and Emperor Long Fist.
- It may conflict with other mods that modify these martial arts, related combat-skill data tables, effect tables, or display logic.
- Breakthrough compatibility uses minimum safeguards and should not override stronger success-rate changes from other mods.
- After changing the game language, fully exit and restart the game.

## License

The source code is licensed under the MIT License. See [LICENSE](LICENSE).

Image assets and third-party game assets are not covered by the MIT License unless explicitly stated. See [ASSET_LICENSE.md](ASSET_LICENSE.md).
