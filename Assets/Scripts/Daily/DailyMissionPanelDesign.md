# MissionPanel 每周任务管理脚本设计

## 目标

给 `MissionPanel` 挂载一个独立脚本，用来管理每日行动地图中的本周任务完成度。任务类型包括：

- 战斗任务
- 废弃仓库
- 支援箱
- 交叉验证

脚本需要支持在 Inspector 面板上配置每项任务的最大需求数值，并提供外部 API 增加完成度、刷新 UI、一键清零。

## 建议脚本

脚本名：

```text
DailyMissionPanelController.cs
```

挂载对象：

```text
DailyOperationMapCanvas/DailyOperationMap/MissionPanel
```

职责边界：

- 只负责本周任务的数据、进度显示、清零和完成度增加。
- 不负责节点移动、不负责卡牌逻辑、不负责打开任务奖励或战斗面板。
- 外部系统通过 API 通知它“某类任务完成了一次”。

## 数据结构

定义任务类型枚举：

```csharp
public enum DailyWeeklyMissionType
{
    Battle,
    AbandonedWarehouse,
    SupportBox,
    CrossValidation
}
```

定义单项任务配置：

```csharp
[Serializable]
public class DailyWeeklyMissionEntry
{
    public DailyWeeklyMissionType missionType;
    public int maxValue = 1;
    public int currentValue;
    public TMP_Text progressText;
    public GameObject completedMark;
}
```

说明：

- `missionType`：任务类型。
- `maxValue`：最大需求数值，可在 Inspector 配置。
- `currentValue`：当前完成度。
- `progressText`：显示 `当前/最大`，例如 `3/5`。
- `completedMark`：可选，达到最大值后显示完成标记。

## Inspector 配置

`DailyMissionPanelController` 中暴露：

```csharp
[SerializeField] private List<DailyWeeklyMissionEntry> missions;
[SerializeField] private bool clampToMax = true;
```

配置示例：

| missionType | maxValue | progressText |
| --- | ---: | --- |
| Battle | 5 | 战斗任务进度文本 |
| AbandonedWarehouse | 3 | 废弃仓库进度文本 |
| SupportBox | 3 | 支援箱进度文本 |
| CrossValidation | 1 | 交叉验证进度文本 |

`clampToMax` 为 true 时，完成度不会超过最大需求。

## 对外 API

增加完成度：

```csharp
public void AddProgress(DailyWeeklyMissionType missionType, int amount = 1)
```

用途：

- 战斗结束后调用 `AddProgress(DailyWeeklyMissionType.Battle)`。
- 抵达废弃仓库节点后调用 `AddProgress(DailyWeeklyMissionType.AbandonedWarehouse)`。
- 抵达支援箱节点后调用 `AddProgress(DailyWeeklyMissionType.SupportBox)`。
- 抵达交叉验证节点后调用 `AddProgress(DailyWeeklyMissionType.CrossValidation)`。

一键清零：

```csharp
public void ClearAllProgress()
```

用途：

- 新一周开始时重置。
- 调试按钮或编辑器测试时重置。

设置某项进度：

```csharp
public void SetProgress(DailyWeeklyMissionType missionType, int value)
```

用途：

- 读档后恢复任务进度。
- 调试时直接设置某项任务完成度。

获取当前进度：

```csharp
public int GetProgress(DailyWeeklyMissionType missionType)
```

判断是否完成：

```csharp
public bool IsCompleted(DailyWeeklyMissionType missionType)
```

判断全部完成：

```csharp
public bool IsAllCompleted()
```

刷新 UI：

```csharp
public void RefreshAll()
```

用途：

- Awake/Start 初始化。
- 外部批量修改后手动刷新。

## UI 刷新规则

每次调用 `AddProgress`、`SetProgress`、`ClearAllProgress` 后，脚本自动刷新对应任务 UI。

显示规则：

```text
currentValue / maxValue
```

示例：

```text
战斗任务 4/5
支援箱 2/3
交叉验证 0/1
```

当 `currentValue >= maxValue`：

- 文本显示最大完成状态，例如 `5/5`。
- 如果配置了 `completedMark`，则显示完成标记。

当 `currentValue < maxValue`：

- 如果配置了 `completedMark`，则隐藏完成标记。

## 和节点事件的连接方式

`DailyMapNode.TriggerStayEvent` 或节点事件监听器在角色最终停留到节点时调用任务 API。

建议由节点事件监听器转换任务类型，不让 `MissionPanel` 直接理解地图节点：

```csharp
public void OnNodeArrived(DailyMapNode node)
{
    switch (node.EventType)
    {
        case DailyMapNodeEventType.Battle1:
        case DailyMapNodeEventType.Battle2:
        case DailyMapNodeEventType.Battle3:
            missionPanel.AddProgress(DailyWeeklyMissionType.Battle);
            break;

        case DailyMapNodeEventType.Resource:
            missionPanel.AddProgress(DailyWeeklyMissionType.AbandonedWarehouse);
            break;

        case DailyMapNodeEventType.ExternalSupport:
            missionPanel.AddProgress(DailyWeeklyMissionType.SupportBox);
            break;

        case DailyMapNodeEventType.PreciseRemoteSensing:
            missionPanel.AddProgress(DailyWeeklyMissionType.CrossValidation);
            break;
    }
}
```

后续如果节点类型和任务类型不是一一对应，可以在 `DailyMapNode` 上额外配置 `DailyWeeklyMissionType`，由节点自己声明抵达后增加哪类任务。

## 推荐实现流程

1. 新建 `DailyMissionPanelController.cs`。
2. 将脚本挂到 `MissionPanel`。
3. 在 Inspector 中配置四个任务项的 `maxValue` 和进度文本。
4. `Awake` 时执行 `RefreshAll()`。
5. 节点最终停留事件调用 `AddProgress()`。
6. 新周开始或调试按钮调用 `ClearAllProgress()`。

## 注意事项

- 完成度增加 API 应该只在角色最终停留节点时调用，路过节点不能增加任务进度。
- `maxValue` 最小应保护为 `1`，避免 UI 出现 `0/0`。
- `amount` 小于等于 0 时可以直接忽略，避免误减进度。
- 如果未来需要存档，保存 `missionType -> currentValue` 即可，`maxValue` 仍由面板配置控制。
