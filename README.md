# Four Arts of the Mystic Pivot

Source project for the Steam Workshop mod **Four Arts of the Mystic Pivot** for **The Scroll of Taiwu**.

This repository contains the mod source code and release metadata. It is not affiliated with or endorsed by the developers or publishers of The Scroll of Taiwu.

Chinese architecture notes for mod authors: [docs/ARCHITECTURE.zh-CN.md](docs/ARCHITECTURE.zh-CN.md).

## Current Release

- Mod version: `1.0.1.5`
- Supported game version: `1.0.29`
- Workshop FileId: `3747599301`
- Author: `Umbrella`

## Features

- Reworks four base sectless Tier 9 martial arts into Divine Tier 1 core arts.
- Keeps low-tier learning, usage, and breakthrough burden for these four arts.
- Adds a four-piece set bonus when all four arts are equipped for Practice.
- Uses independent empty effect shells plus backend patches for the four core passives, avoiding changes to the original vanilla effect entries.
- Reworks Myriad-Devouring Blood-Rift Fist into an independent backend passive: Normal Attack hits can gain Blood-Rift stacks and trigger a forced-hit, guaranteed-critical pursuit that does not trigger bounce damage.
- Gives player commands priority over Normal Attacks while the four-piece set is active.
- Adds in-game set tooltip support with Alt details, while suppressing the large set tooltip during combat.
- Supports Chinese and English in-game text at game startup.

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
   - `Description` metadata lines for mod version and supported game version
   - version heading in the description when needed
2. Keep `DetailImageList = { }` unless the workshop detail-image order intentionally needs to be replaced.
3. Build both projects.
4. Sync `dist/Config.Lua`, `dist/Settings.Lua`, `dist/Plugins/`, and required image assets to:

```text
C:\Program Files (x86)\Steam\steamapps\common\The Scroll Of Taiwu\Mod\XuanShuFourArts
```

5. Upload through the in-game mod manager.

## Compatibility Notes

- This mod patches the four base sectless martial arts: Chant of Abundance, Minute Leap, Yin-Yang Iron Skin, and Emperor Long Fist.
- It may conflict with other mods that modify these martial arts, related combat-skill data tables, effect tables, or display logic.
- Breakthrough compatibility uses minimum safeguards and should not override stronger success-rate changes from other mods.
- After changing the game language, fully exit and restart the game.

## License

The source code is licensed under the MIT License. See [LICENSE](LICENSE).

Image assets and third-party game assets are not covered by the MIT License unless explicitly stated. See [ASSET_LICENSE.md](ASSET_LICENSE.md).
