# Release Checklist

Use this checklist before every workshop update.

## Version Metadata

- Update `dist/Config.Lua` top-level `Version`.
- Update `dist/Config.Lua` top-level `GameVersion`.
- Update `dist/Config.Lua` description metadata:
  - `Mod Version`
  - `Supported Game Version`
  - version heading, if present
- Update `README.md` current release block.
- Add a `CHANGELOG.md` entry.

## Workshop Images

- Keep `DetailImageList = { }` unless the workshop detail-image order must be intentionally replaced.
- Do not point `DetailImageList[1]` at the square cover image.
- Keep `Cover` and `WorkshopCover` only for the local cover and workshop thumbnail.

## Build

```powershell
dotnet build src/CoreSkillGrowth.Backend/CoreSkillGrowth.Backend.csproj
dotnet build src/CoreSkillGrowth.Frontend/CoreSkillGrowth.Frontend.csproj
```

Both builds should finish with 0 errors.

## Sync To Game Mod Folder

Sync release files to:

```text
C:\Program Files (x86)\Steam\steamapps\common\The Scroll Of Taiwu\Mod\XuanShuFourArts
```

Required files:

- `dist/Config.Lua`
- `dist/Settings.Lua`
- `dist/Plugins/XuanShuFourArtsB.dll`
- `dist/Plugins/XuanShuFourArtsF.dll`
- `dist/Plugins/XuanShuFourArtsB.deps.json`
- required cover image assets

## Sanity Check

- Open the in-game upload screen.
- Confirm mod version and supported game version match `dist/Config.Lua`.
- Confirm current game version is compatible.
- Confirm workshop detail images have not been replaced or reordered.
