# 戴森球计划 - 战场分析基站收集太空残骸 Mod

## 简介

![收集太空残骸](https://github.com/unieagle/dsp-mod-BattlefieldCollectSpaceJunk/blob/main/img/collecting%20space%20debris.png?raw=true)

本 Mod 让**击毁黑雾太空敌舰**时产生的战利品，从击毁位置**坠向战场分析基站所在行星**，由基站范围内拾取，实现太空战利品自动归集到基站。

### ✨ 核心特性

- 🚀 **太空掉落归集**：黑雾太空敌舰，按地面掉落表抽选战利品x100（可配置），从击毁点坠向本星系内「有战场基站的最近行星」
- 📍 **尾迹与落点**：坠落物带尾焰、落点优先在基站附近（有基站时），无基站时落行星表面

### 使用场景

在太空与黑雾交战时，击毁敌舰后战利品会以「坠落物」形式飞向你在该星系建有**战场分析基站**的行星，进入基站拾取范围后自动进站，便于集中处理或配合「战场分析基站配送」类 Mod 做物流。

### English

This mod makes **loot from destroyed Dark Fog space ships** fall from the kill position **toward the planet that has a Battlefield Analysis Base**, and get picked up within the base’s range, so space loot is collected at the base automatically.

#### Features

- **Space loot collection**: When Dark Fog space ships are destroyed, loot is rolled from the ground drop table ×100 and falls from the kill point toward the nearest planet in the system that has a Battlefield Analysis Base.
- **Trail and landing**: Falling items have a trail; they land near the base when one exists, or on the planet surface otherwise.

#### Use case

When fighting the Dark Fog in space, loot from destroyed ships flies as “falling trash” toward a planet in that star system where you have a **Battlefield Analysis Base**, and is auto-collected when in range, for central handling or for use with mods like “Battlefield Analysis Base Deliver”.

---

## 安装

1. 确保已安装 **BepInEx 5.x** (x64)
2. 将 `BattlefieldAnalysisBaseCollectSpaceJunk.dll` 放入 `BepInEx\plugins\` 文件夹
3. 启动游戏

---

## 配置与日志

- 配置文件：`BepInEx\config\un1eagle.battlefieldanalysisbasecollectspacejunk.cfg`
- **EnableDebugLog**（默认 `false`）：为 `true` 时在日志中输出详细调试信息（击毁/战利品明细/落点/从未掉落统计等），用于排查问题；正常使用建议保持 `false`，仅保留加载与错误信息。
- 日志位置：`BepInEx\LogOutput.log`

---

## 兼容性

- ✅ 与现有存档兼容
- ✅ 可与「战场分析基站配送」等 Mod 共存（本 Mod 只负责生成坠落物并落向基站，拾取与配送由游戏或其它 Mod 处理）
- ⚠️ 单堆数量 &gt; 255 时，若在拾取前存盘，读档后该堆数量会变为 255（游戏 TrashObject 存盘用 byte），可忽略

---

## 构建与打包

```bash
dotnet build -c Release
.\pack-mod.ps1
# 或指定输出目录: .\pack-mod.ps1 -OutputDir ".\dist"
```

输出：`BattlefieldAnalysisBaseCollectSpaceJunk-<version>.zip`（内含 manifest.json、README.md、icon.png、DLL）
