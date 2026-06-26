# Release Checklist

Use this checklist before every workshop update.

## Version Metadata

- Update `dist/Config.Lua` top-level `Version`.
- Update `dist/Config.Lua` top-level `GameVersion`.
- Preserve the current Steam Workshop default English `Title` and `Description` unless the user explicitly asks to rewrite workshop page text.
- Before changing workshop-facing metadata, read the current Steam metadata first and use the English/default metadata as the source:
  - Preferred raw metadata API: `https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/` with `publishedfileids[0]=3747599301`
  - English: `https://steamcommunity.com/sharedfiles/filedetails/?id=3747599301&l=english`
  - Simplified Chinese: `https://steamcommunity.com/sharedfiles/filedetails/?id=3747599301&l=schinese`
- Do not replace the default English workshop metadata with Chinese text. Steam uses the default language page as English when English localization exists.
- Update only the needed version fields inside the preserved description text:
  - `Mod Version`
  - `Supported Game Version`
  - version heading, if present
- Update `README.md` current release block.
- Add a `CHANGELOG.md` entry.

## Workshop Images

- Keep `DetailImageList = { }` unless the workshop detail-image order must be intentionally replaced.
- Do not point `DetailImageList[1]` at the square cover image.
- Keep `Cover` and `WorkshopCover` only for the local cover and workshop thumbnail.
- Do not let a release sync alter workshop image order or localized title/description content.

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
