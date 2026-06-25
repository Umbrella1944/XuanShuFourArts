# 中文架构说明

这份文档面向想阅读、学习或二次开发本 MOD 的中文 MOD 作者。它只解释项目结构和实现思路，不替代源码本身。

## 项目定位

`Four Arts of the Mystic Pivot` 是《太吾绘卷》的功能型 Harmony MOD。它将四个无门无派低阶功法重做为一组长期使用的核心四件套：

- `玄息归骸经`，原型为 `沛然诀`
- `照影游身步`，原型为 `小纵跃功`
- `两仪还形功`，原型为 `水火映气功`
- `万噬血隙拳`，原型为 `太祖长拳`

本项目的重点不是简单改表，而是为四个功法建立独立的后端被动逻辑，并额外提供四件套属性、UI 提示、突破兼容和发布流程。

## 目录结构

```text
src/
  CoreSkillGrowth.Backend/    后端 Harmony Patch，负责战斗逻辑和数值生效
  CoreSkillGrowth.Frontend/   前端 Harmony Patch，负责 UI 和鼠标提示
  CoreSkillGrowth.Shared/     前后端共用的功法配置修改
dist/
  Config.Lua                  游戏 MOD 配置和创意工坊描述
  Settings.Lua                默认设置
  Plugins/                    本地构建输出目录，不提交到 Git
  *.jpg                       封面和宣传图片
```

`Backend` 和 `Frontend` 是两个独立插件，分别生成：

- `XuanShuFourArtsB.dll`
- `XuanShuFourArtsF.dll`

游戏加载时会同时加载这两个 DLL。

## 启动流程

两个插件入口分别是：

- `src/CoreSkillGrowth.Backend/BackendPlugin.cs`
- `src/CoreSkillGrowth.Frontend/FrontendPlugin.cs`

入口做两件事：

1. 创建 Harmony 实例并 `PatchAll`。
2. 调用 `CoreSkillGrowthConfigPatch.ApplyAll(...)` 修改功法配置和注册显示用特效。

`CoreSkillGrowthConfigPatch` 位于 `src/CoreSkillGrowth.Shared/CoreSkillGrowthConfigPatch.cs`。它被前后端共同引用，确保两边看到的功法 ID、特效 ID 和文本一致。

## DR 是什么

本项目里的 `DR` 指 `Decoupled Reimplementation`，中文可以理解为“解耦重实现”。

旧做法通常是直接复用或修改游戏原本的 `SpecialEffect` 词条。这样做简单，但容易和本体逻辑、其他 MOD、原功法效果互相影响。

本项目采用的方式是：

1. 为四个功法注册自己的空 `SpecialEffect` 显示壳。
2. 让这个显示壳只负责名字、描述、BUFF 显示和提示。
3. 真正的战斗效果由后端 Harmony Patch 独立判断和执行。

这样可以减少对原版特效项的依赖，也更容易控制兼容风险。

## 四个功法的实现分工

### 玄息归骸经

主要文件：

- `src/CoreSkillGrowth.Backend/PeiRanAutoHealSpeedPatch.cs`
- `src/CoreSkillGrowth.Shared/CoreSkillGrowthConfigPatch.cs`

效果逻辑：

- 战斗开始时检测角色是否装备该功法。
- 向角色的伤势自动恢复速度列表里加入本 MOD 自己的恢复项。
- 默认每 10 次自动恢复更新，实际生效 1 次。
- 四件套时改为每 7 次自动恢复更新，实际生效 1 次。
- 战斗结束时清理运行时状态。

这个设计是为了避免恢复过快，同时保留“持续自行消除伤势标记”的核心定位。

### 照影游身步

主要文件：

- `src/CoreSkillGrowth.Backend/DecoupledCoreSkillEffectsPatch.cs`
- `src/CoreSkillGrowth.Backend/FourSetActiveSkillBonusPatch.cs`
- `src/CoreSkillGrowth.Shared/CoreSkillGrowthConfigPatch.cs`

效果逻辑：

- 当身法生效时，受到直接伤害会优先消耗脚力抵消伤害。
- 脚力不足时，剩余伤害正常结算。
- 四件套时，提高该身法持续期间的施展移动速度加成，当前为 `+40%`。

注意：它不是自动恢复脚力，而是把直接伤害转换为脚力消耗。

### 两仪还形功

主要文件：

- `src/CoreSkillGrowth.Backend/DecoupledCoreSkillEffectsPatch.cs`
- `src/CoreSkillGrowth.Backend/FourSetActiveSkillBonusPatch.cs`
- `src/CoreSkillGrowth.Shared/CoreSkillGrowthConfigPatch.cs`

效果逻辑：

- 防御功结束时触发。
- 对残毁部位的新增伤势减少 1 层。
- 随后将全身新伤和旧伤随机平均分配。
- 四件套时，提高防御功持续期间提供的御体、御气、卸力、拆招、闪避效果，当前为 `+40%`。

### 万噬血隙拳

主要文件：

- `src/CoreSkillGrowth.Backend/FourSetNormalAttackPowerPatch.cs`
- `src/CoreSkillGrowth.Shared/CoreSkillGrowthConfigPatch.cs`

效果逻辑：

- 运用者普通攻击命中后，有 `20%` 机率触发血隙。
- 触发时获得 2 层血隙，并准备 1 次血隙追击。
- 每层血隙使血隙追击威力 `+200%`，最多 10 层。
- 血隙追击必中、必重创，不触发反震。
- 正常普通攻击仍然会正常触发反震。
- 进入追击伤害结算时消耗全部血隙层数。
- 如果追击被运用者的主动指令或其他情况中断，则损失 1 层血隙。

四件套时还提供：

- 普通攻击威力 `+100%`
- 血隙触发的保底机制，连续未触发后机率逐步提高，上限为 `100%`
- 使运用者的主动指令优先于普通攻击

## 四件套效果

主要文件：

- `src/CoreSkillGrowth.Backend/FourSetBonusPatch.cs`
- `src/CoreSkillGrowth.Backend/FourSetActiveSkillBonusPatch.cs`
- `src/CoreSkillGrowth.Backend/FourSetNormalAttackPowerPatch.cs`
- `src/CoreSkillGrowth.Frontend/FourSetTooltipPatch.cs`

四件套判定条件：

角色同时装备四个核心功法：

- `玄息归骸经`
- `照影游身步`
- `两仪还形功`
- `万噬血隙拳`

四件套效果包括：

- 主战斗属性 `+15%`
- 普通攻击威力 `+100%`
- 血隙保底机制
- 身法主动效果 `+40%`
- 绝技主动防御效果 `+40%`
- 内功被动恢复从 10 次结算 1 次提升为 7 次结算 1 次
- 战斗中运用者的主动指令优先于普通攻击

## UI 说明

主要文件：

- `src/CoreSkillGrowth.Frontend/FourSetTooltipPatch.cs`

UI 逻辑负责：

- 在功法界面显示四件套进度。
- 按住 `Alt` 显示详细套装效果。
- 在战斗场景中屏蔽大型套装提示，避免遮挡技能内容。
- 兼容不同功法提示面板，包括旧式 `MouseTipCombatSkill` 和新式 `TooltipCombatSkill`。

UI 只负责显示，不负责后端数值生效。

## 突破兼容

主要文件：

- `src/CoreSkillGrowth.Backend/CoreSkillBreakCompatibilityPatch.cs`

目的：

四个功法虽然显示为高品级，但角色不应该因为神一品定位而在开局遇到突破困难。因此本 MOD 只针对这四本功法，提供低阶突破负担兼容。

兼容原则：

- 只影响四个核心功法。
- 经验消耗使用更低值。
- 可用步数和成功率只做保底，不压低其他 MOD 提供的更高数值。
- 如果其他 MOD 提供更强的突破成功率，本 MOD 不应该覆盖它。

## 哪些数值可以安全调整

相对安全的数值：

- `FourSetBonusPatch.cs` 中的 `MajorCombatPropertyBonusPercent`
- `FourSetActiveSkillBonusPatch.cs` 中的 `ActiveSkillBonusPercent`
- `FourSetNormalAttackPowerPatch.cs` 中的触发率、层数上限、每层威力、普通攻击威力、保底参数
- `DecoupledCoreSkillEffectsPatch.cs` 中的绝技残毁部位伤势减少层数
- `PeiRanAutoHealSpeedPatch.cs` 中的内功恢复节流次数
- `CoreSkillGrowthConfigPatch.cs` 中的功法描述、属性列表、基础功法参数

需要谨慎的内容：

- Harmony Patch 的目标方法签名。
- 普通攻击状态机相关逻辑。
- `SpecialEffect` 注册和功法 ID。
- UI 浮动面板的生命周期和战斗场景屏蔽逻辑。

## 兼容性边界

这个 MOD 可能和以下类型的 MOD 冲突：

- 修改同四个功法配置的 MOD。
- 修改相关功法表、特效表、战斗结算方法的 MOD。
- 修改同一批 UI Tooltip 的 MOD。
- 大幅改写普通攻击流程或战斗状态机的 MOD。

本项目已经尽量将核心效果独立化，但 Harmony MOD 无法完全避免 Patch 层面的冲突。

## 发布注意事项

发布前至少检查：

- `dist/Config.Lua` 里的 `Version`
- `dist/Config.Lua` 里的 `GameVersion`
- 创意工坊描述中的版本号
- `DetailImageList = { }` 是否保持为空
- 前后端 DLL 是否重新构建并同步到游戏 MOD 目录
- 游戏内是否能正常加载并进入战斗

不要把 `dist/Plugins/*.dll` 提交到 Git 仓库。它们是本地构建产物，不是源码。

## 给普通 MOD 作者的阅读顺序

推荐按这个顺序读：

1. `README.md`
2. 本文档
3. `src/CoreSkillGrowth.Shared/CoreSkillGrowthConfigPatch.cs`
4. `src/CoreSkillGrowth.Backend/FourSetBonusPatch.cs`
5. `src/CoreSkillGrowth.Backend/FourSetNormalAttackPowerPatch.cs`
6. `src/CoreSkillGrowth.Backend/DecoupledCoreSkillEffectsPatch.cs`
7. `src/CoreSkillGrowth.Frontend/FourSetTooltipPatch.cs`

如果只是想改数值，优先看常量和 `CoreSkillGrowthConfigPatch.cs`。如果想学习战斗机制，再看后端 Patch。
