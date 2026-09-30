# Release Checklist

Use this checklist before every workshop update.

## Version Metadata

- Update `dist/Config.Lua` top-level `Version`.
- Update `dist/Config.Lua` top-level `GameVersion`.
- The author alone edits Workshop introduction text. Automated maintenance may update version numbers only; do not insert release notes, shorten, rewrite, or reorganize the introduction. Synchronize author-edited live text verbatim when updating the repository.
- Before changing workshop-facing metadata, read the current Steam metadata first and use the English/default metadata as the source:
  - Preferred raw metadata API: `https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/` with `publishedfileids[0]=3747599301`
  - English: `https://steamcommunity.com/sharedfiles/filedetails/?id=3747599301&l=english`
  - Simplified Chinese: `https://steamcommunity.com/sharedfiles/filedetails/?id=3747599301&l=schinese`
- Do not replace the default English workshop metadata with Chinese text. Steam uses the default language page as English when English localization exists.
- Update only the needed version fields inside the preserved description text:
  - `Mod Version`
  - `Supported Game Version`
  - version heading, if present
- Before every upload, check the local upload source file:
  - `C:\Program Files (x86)\Steam\steamapps\common\The Scroll Of Taiwu\Mod\XuanShuFourArts\Config.Lua`
  - `C:\Program Files (x86)\Steam\steamapps\workshop\content\838350\3747599301\Config.Lua`
- These local `Config.Lua` files must keep the default `Title` and `Description` in English. If the in-game mod manager rewrites them into Chinese after an upload, re-read the current author-edited English/default Workshop text before restoring it. Never overwrite newer author edits with an older `dist/Config.Lua`.
- Check the decoded description's UTF-8 byte length against Steam's 8,000-byte limit. If it is too long, report the measured length to the author; do not shorten the text automatically.
- Do not upload from a Chinese-rewritten local `Config.Lua`; that will overwrite the Steam default/English workshop page with Chinese.
- Update `README.md` current release block.
- Add a `CHANGELOG.md` entry.

## Workshop Images

- Keep `DetailImageList = { }` unless the workshop detail-image order must be intentionally replaced.
- Do not point `DetailImageList[1]` at the square cover image.
- Keep `Cover` and `WorkshopCover` only for the local cover and workshop thumbnail.
- Do not let a release sync alter workshop image order or localized title/description content.

## Steam Change Notes

- Use Steam Change Notes / update-section format: `[h1]Title[/h1]` followed directly by Markdown-style bullet lines beginning with `*`.
- Do not use `[list]` or `[*]` in Change Notes.
- Keep one blank line between major bullet groups when the note is detailed.
- `UpdateLogList` in `Config.Lua` records timestamps only; it does not store the visible update-note body.
- Recommended template:

```text
[h1]1.0.2.0 Compatibility Update[/h1]

* Updated supported game version to 1.1.23.
* Updated all four arts for the new direct/reverse effect ID field types.

* Updated Blood-Rift eligibility to use the current Normal Attack states after removal of a legacy combat flag.

* Made the removed legacy equipment screen optional so frontend initialization can continue with the current equipment screen.

* Preserved the four arts, set bonuses, balance values, and Chinese/English tooltips.
```

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
