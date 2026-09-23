# LIVE A EVIL 工程结构与架构方案

> 目标：把现在的「能跑的原型」逐步整理成「正规工程的骨架」，
> 每个阶段独立可验证、可随时停下，且不打断正在做的玩法开发。

---

## 一、正规 Unity 工程的目录骨架

```
<ProjectRoot>/
├─ Assets/
│  ├─ _Project/                     ← 自己的东西集中在这（下划线排最前，和包资源分开）
│  │  ├─ Art/         Sprites/ Models/ Materials/ Animations/ VFX/
│  │  ├─ Audio/       BGM/ SE/
│  │  ├─ Data/        Characters/ Cards/ Boards/ Balancing/   ← ScriptableObject 配置
│  │  ├─ Prefabs/     Characters/ Tiles/ UI/ Managers/
│  │  ├─ Scenes/      Boot / Title / Draft / Battle / Result
│  │  ├─ Settings/    URP Asset / Input Actions / Quality
│  │  ├─ UI/          Sprites/ Fonts/
│  │  └─ Scripts/
│  │     ├─ Runtime/
│  │     │  ├─ Core/            纯规则：伤害、范围、牌堆、随机、状态机（尽量不 using UnityEngine）
│  │     │  ├─ Domain/          Character / Card / Board / Tile 的数据与行为
│  │     │  ├─ Gameplay/        回合流程、技能系统、AI、计时器
│  │     │  ├─ Presentation/    View、HUD、摄像机、特效
│  │     │  └─ Infrastructure/  存档、输入、资源加载、日志
│  │     ├─ Editor/             编辑器工具（独立 asmdef，不进打包）
│  │     └─ Tests/              EditMode/（纯规则）PlayMode/（流程）
│  ├─ Plugins/  ThirdParty/     第三方资源，只读不改
│  └─ TextMesh Pro/             包自带
├─ Packages/
├─ ProjectSettings/
├─ docs/                        规则映射、架构决策记录（ADR）、上手说明
├─ .gitignore  README.md
```

**目录本身不产生价值，产生价值的是下面这套约束。**

---

## 二、比目录更重要的 9 条工程约束

| # | 约束 | 说明 |
|---|---|---|
| 1 | **程序集分层（asmdef）** | Core ← Gameplay ← Presentation 单向依赖。UI 能引 Core，Core 引 UI 就编译不过 —— 反向依赖在编译期暴露，而不是运行时 NRE |
| 2 | **显式装配入口** | Boot 场景 + `GameContext`（服务容器）统一持有 Manager；不在业务代码里满场 `FindFirstObjectByType` |
| 3 | **事件解耦** | Manager 之间不互相直接调用，改用 C# event / ScriptableObject EventChannel |
| 4 | **数据驱动** | 数值全在 ScriptableObject / CSV；代码里只留结构，不留魔数 |
| 5 | **Prefab 化** | 角色、格子、UI 面板都是 Prefab，代码只 `Instantiate`；不 `CreatePrimitive` + `AddComponent` |
| 6 | **显式状态机** | 回合流程用 FSM 表达（卡组设置→移动→攻击→计时→结算），不靠散落各处的 `if (state == ...)` |
| 7 | **表现与逻辑分离** | 逻辑改 Model 并抛事件，View 订阅后刷新；逻辑不直接改颜色/材质 |
| 8 | **测试** | 纯规则（伤害公式、范围形状、击飞、计时器）走 EditMode 测试，改公式先跑测试 |
| 9 | **人话文档** | README（怎么跑）+ docs/ADR（为什么这么做）；日志走统一门面并分级 |

---

## 三、本工程现状 vs 标准（实测数据）

| 维度 | 现状 | 标准 | 现在会踩的坑 |
|---|---|---|---|
| 目录 | `Assets/{Cards, Characters, Editor, Scenes, Scripts}`，Scripts 按名词分（Board/Card/Character/Core/Gameplay） | `_Project/{Art, Audio, Data, Prefabs, Scenes, Settings, Scripts/Runtime/{Core,Domain,Gameplay,Presentation,Infrastructure}, Editor, Tests}` | 美术/UI/Prefab 没有落脚点；分层看不出依赖方向 |
| 程序集 | **0 个 asmdef**，全在 Assembly-CSharp | 5~6 个 asmdef | 任何脚本能引任何脚本；改一行全量重编译（62 脚本 1.9s，还能忍，但会越来越慢） |
| 依赖装配 | **39 处** `FindFirstObjectByType`，Manager 互相直接调用 | Boot + GameContext + 事件 | 加第二个战场/多场景就散架；漏挂组件（AttackManager 已漏过一次） |
| 数据 | 卡牌/角色**已用 SO**（好）；但棋盘 7×7、格缩放 0.95、颜色、UI 像素偏移写在代码 | 全进 GameConfig / BattleRules | 调数值要改代码重编译；和规则书对不上 |
| 场景 | 22 个根对象平铺，Manager 各自独立 | 层次树 / Managers 父节点 / Prefab | 场景越加越乱，容易漏引用 |
| 运行时对象 | `CreatePrimitive`×4、`AddComponent`×3 造角色和格子 | Prefab + Instantiate（+对象池） | 改外观要改代码，没法在 Inspector 调 |
| UI | **3 处 OnGUI**（手牌/部署/选秀） | uGUI / UI Toolkit + Prefab | IMGUI 抢点击、和 Canvas 抢层级、无法复用美术 |
| 流程 | `BattleState` + 各 Manager 自己判断 | 显式 FSM | 已出过真实 bug：移动后按两次 Q 能再走一次（本次已修） |
| 表现 | 逻辑直接改颜色（`SetNormalColor` / `SetVisualState`） | Model 变更 → View 刷新 | 规则没法脱离引擎测试 |
| 测试 | **0** | EditMode + PlayMode | 每次改公式只能手点验证 |
| 日志 | **183 处** `Debug.Log` | GameLog 门面 + 分级 + `[Conditional]` | 发布版带一堆日志 |
| 文档 | 无 README | README + docs | 隔两周自己都忘了怎么跑 |

**已经做对的地方**（不用改）：卡牌/角色用 ScriptableObject、`.gitignore` 完整（Library/Temp/obj/csproj 都忽略）、`.meta` 全部入库（260 个）、ForceText 序列化、规则书→卡面数值已映射。

---

## 四、迁移方案（分阶段，每步独立可验证）

| 阶段 | 内容 | 工作量 | 风险 | 价值 |
|---|---|---|---|---|
| **P0** | 写 README（怎么跑：选秀→部署→战斗，F1/Q/K 快捷键）+ docs/ | 10 分钟 | 零 | 上手成本 |
| **P1** | 目录归位：`Assets/_Project/{Data,Scripts,Scenes,Editor}`；`Cards`→`Data/Cards`、`Characters`→`Data/Characters`；`Scripts` 按层拆到 `Runtime/{Domain,Gameplay,Presentation,Infrastructure}`。**只移动不改代码** | 1 小时 | 低 | 后续一切的前提 |
| **P2** | asmdef 分层：先 `LIVE.Runtime` + `LIVE.Editor` + `LIVE.Tests`，稳定后把 Runtime 拆 `Core`（不 using UnityEngine）/`Gameplay`/`Presentation` | 2 小时 | 低 | 编译期挡住反向依赖 |
| **P3** | 数据收口：`GameConfig`（棋盘尺寸、格缩放、颜色、UI 尺寸）+ `BattleRules`（伤害公式、弱化层数、击飞距离、抽牌张数） | 半天 | 低 | 改数值不再改代码 |
| **P4** | Prefab 化：`Character.prefab` / `Tile.prefab` / 手牌 UI Prefab + `PrefabFactory`，干掉 `CreatePrimitive`+`AddComponent` | 半天 | 中 | 美术可迭代 |
| **P5** | 依赖装配：Boot 场景 + `GameContext.Get<T>()`，逐步替换 39 处 Find；Manager 之间改事件订阅 | 1 天 | 中 | 可扩展、可多场景 |
| **P6** | 回合 FSM：`RoundFlow`（准备→卡组设置→交替輪→移动→攻击→计时→回合结算→抽牌/胜负），把 `HasMoved/HasAttacked` 判断收进去，落规则书的胜负条件 | 1~2 天 | 中 | 规则正确性的地基 |
| **P7** | UI 迁移：OnGUI → Canvas Prefab（手牌 6 槽、信息面板、部署/选秀按钮），复用已有 `HandUIManager`/`CardUI` 壳 | 1 天 | 中 | 可出 Demo |
| **P8** | 测试：EditMode 覆盖伤害公式、Square/Cross/米字 范围形状、击飞、计时器取消、移动卡消耗 | 持续 | 低 | 改公式不再心慌 |

### 推荐节奏（单人项目，别一次性全上）

1. **现在做**：P0 + P1 + P2 + P3（合计约一天）—— 机械性工作，不动玩法逻辑，做完立刻少一半后期返工。
2. **玩法补齐时做**：P6（回合/胜负框架本来就要重写，一并做最省）+ P5。
3. **出 Demo 前做**：P7 + P4。
4. **P8** 随时插入，先把「伤害公式 / 范围形状 / 移动卡消耗」三条测起来。

### 单人项目的取舍（重要）

- **不要上**：完整 DI 框架（Zenject/VContainer）、ECS、过度抽象的事件总线、ADR 仪式感 —— 对单人原型是纯负担。
- **值得上**：asmdef 分层、数据驱动、Prefab 化、回合 FSM、纯规则单测。这五样是"省时间"的，其余多半是"花时间"的。
- 判断标准：**这一步能否减少我未来改一个玩法要动的文件数？** 能就做，不能就跳过。
