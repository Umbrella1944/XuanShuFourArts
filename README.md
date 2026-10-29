# 玄枢四诀

[中文](README.md) | [English](README.en.md) | [한국어](README.ko.md)

《太吾绘卷》功法套装 MOD「玄枢四诀」的源码与发布工程。本项目不是游戏官方或发行方的附属作品。

中文架构说明见 [docs/ARCHITECTURE.zh-CN.md](docs/ARCHITECTURE.zh-CN.md)。

## 当前版本

- MOD 版本：`1.1.0.0`
- 支持游戏版本：`1.1.23`
- 创意工坊 FileId：`3747599301`
- 作者：`Umbrella`

兼容性验证与实机检查边界见 [1.1.23 维护记录](docs/MAINTENANCE-1.1.23.zh-CN.md)。本地部署与离线检查已完成，作者已于 2026-09-29 发布工坊更新；完整游戏内验证仍需继续补充。

## 主要功能

- 将四门无门无派低阶功法改造为神阶一品核心功法，并保留低阶学习、运功和突破负担。
- 同时运功四诀可获得套装加成。
- 万噬血隙拳可通过完整普通攻击累积血隙并触发必中、必重创追击，追击不触发反震。
- 支持按使用者分别计算成长倍率，成长来源包括精纯、轮回、年龄与寿命比例、已学功法。
- 太吾与 NPC 使用同一套功法效果和成长算法，可配置 NPC 血隙与套装普攻加成。
- 支持强度滑条、可选调试日志和限流诊断。
- 游戏内文本支持 `CN/CNH/EN/KO`，未知语言回退中文。

## 强度与成长设置

在游戏 MOD 管理中打开「详细设置」。四项单诀与套装可分别调整，默认强度均为 100。强度调整的是对应收益，不是最终伤害倍率；消耗、要求、持续时间、层数上限和核心判定规则保持不变。修改后需要完整重启游戏。

成长模式开启时，实际强度为「强度」页数值乘以成长倍率。年龄按寿命曲线变化，在寿命中点达到峰值后逐渐衰减；其他成长项按各自权重累计。血隙强度为 50 时，基础触发率为 10%，每层追击威力为 +75%。

范围与验证见 [强度设置说明](docs/STRENGTH-SETTINGS.zh-CN.md)。本地部署保留玩家现有设置，旧的空设置文件使用默认值。

## 目录结构

```text
src/          C# 工程与源码
tools/        构建、验证和发布脚本
docs/         开发与维护文档
deploy/       发布配置、默认设置模板和展示资源
.tmpbuild/    本机构建输出，不进入 Git
.tmpdeploy/   发布候选暂存，不进入 Git
.decompiled/  本地接口快照，不随 MOD 发布
```

历史备份保存在源码树外的受控归档位置，不覆盖旧版本。

## 构建

需要本机安装《太吾绘卷》和 .NET SDK 8.0 或更高版本。工程默认从以下位置读取游戏程序集：

```text
C:\Program Files (x86)\Steam\steamapps\common\The Scroll Of Taiwu
```

游戏安装在其他位置时，通过 `GameDir` 显式传入：

```powershell
dotnet build src/CoreSkillGrowth.Backend/CoreSkillGrowth.Backend.csproj -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\The Scroll Of Taiwu"
dotnet build src/CoreSkillGrowth.Frontend/CoreSkillGrowth.Frontend.csproj -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\The Scroll Of Taiwu"
```

也可以在构建前设置 `TAIWU_GAME_DIR`。

构建输出位于 `.tmpbuild/<Configuration>/Plugins/`。Debug 仅用于离线检查，不部署到游戏目录。

## 发布流程

1. 更新 `deploy/Config.Lua` 的版本、支持游戏版本及保留介绍中的对应版本字段。
2. 保留工坊现有标题、介绍、详情图顺序和默认英文元数据，不用本地中文介绍覆盖。
3. 构建前后端 Release。
4. 从 Release 输出和 `deploy` 资源在 `.tmpdeploy` 组装候选包。
5. 候选包只使用默认 `deploy/Settings.Lua` 模板，不包含玩家修改后的本机设置。
6. 校验候选包内容、目录结构和 SHA-256 后，再通过游戏内 MOD 管理上传。

## 本地开发与线上目录

- 本仓库所在工作副本是唯一的源码、差异和提交依据，所有修改必须先在 `src`、`tools`、`docs` 或 `deploy` 中完成。
- `SteamLibrary\steamapps\workshop\content\838350` 只用于拉取线上版本，不用于测试部署、维护源码或生成提交。
- 本机测试统一部署到 `The Scroll Of Taiwu\Mod\XuanShuFourArts`，先构建再部署，不从该目录反向维护源码。
- 本地开发版与工坊订阅版不得同时启用。
- 历史 ZIP 保存在 `E:\Work\VibeCoding\TaiWuDev\Archives`，不覆盖旧版本。

## 兼容性说明

- 修改沛然决、小纵跃功、水火引气功、太祖长拳的 MOD 可能与本 MOD 冲突。
- 修改相关功法表、特效表、战斗结算或提示界面的 MOD 也可能存在兼容问题。
- 突破兼容只提供最低保障，不覆盖其他 MOD 提供的更高成功率。
- 切换游戏语言后需要完整重启游戏。

## 许可

源码采用 MIT License，见 [LICENSE](LICENSE)。图片及第三方游戏资源不包含在该许可内，见 [ASSET_LICENSE.md](ASSET_LICENSE.md)。
