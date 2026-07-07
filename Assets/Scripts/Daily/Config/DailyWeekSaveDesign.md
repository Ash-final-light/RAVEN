# Daily Week Save Design

## 目标

Daily 系统需要支持“进入下一天”和“按天回档”。

- 点击进入下一天按钮后，更新当前星期几的 `TMP_Text`，并保存当天 Daily 状态。
- 点击存档按钮后，显示已经保存过的星期数据。
- 选择某一天后，读取该天存档并回档。
- 回档后删除被选择日期之后的所有存档，避免玩家回到过去后还能保留未来进度。

## 总体结构

建议新增一个总控脚本：

```csharp
DailyWeekSaveController
```

推荐挂载位置：

```text
DailyOperationMap
```

它负责：

- 管理当前是第几天。
- 刷新星期文本。
- 调用 Daily 存档采集。
- 写入 JSON 文件。
- 读取 JSON 文件。
- 生成存档按钮列表。
- 回档后删除后续天数。

不要把这些逻辑分别塞进 `DailyMissionPanelController`、`DailyMapStepController`、`DailyMapNode` 或卡牌脚本里。那些脚本只负责导出和应用自己的状态。

## 保存格式

使用一个总 JSON 文件保存整周数据：

```text
Application.persistentDataPath/daily_week_save.json
```

不建议一天一个文件。因为回档时要删除后面几天，一个总文件更容易维护顺序和版本。

## 数据结构

在 `Daily.Config` 命名空间下扩展：

```csharp
[Serializable]
public class DailyWeekSaveData
{
    public int version = 1;
    public int currentDayIndex;
    public List<DailyDaySaveData> days = new List<DailyDaySaveData>();
}
```

```csharp
[Serializable]
public class DailyDaySaveData
{
    public int dayIndex;
    public string weekName;
    public DailySaveData daily;
}
```

字段说明：

| 字段 | 说明 |
|---|---|
| `currentDayIndex` | 当前停留在第几天，建议从 `0` 开始 |
| `days` | 已保存的每天快照 |
| `dayIndex` | 存档对应的天数索引 |
| `weekName` | 用于 UI 显示，例如 `星期一` |
| `daily` | 当前已有的 `DailySaveData`，包含任务、步数、节点、卡牌 |

星期显示可以用数组：

```csharp
private readonly string[] weekNames =
{
    "星期一",
    "星期二",
    "星期三",
    "星期四",
    "星期五",
    "星期六",
    "星期日"
};
```

## 进入下一天按钮逻辑

按钮：

```text
Btn_NextDay
```

绑定方法：

```csharp
public void EnterNextDay()
```

执行顺序：

1. 如果正在移动，禁止进入下一天。
2. 采集当前 Daily 状态，保存为当前天快照。
3. `currentDayIndex++`。
4. 限制最大天数，最多到星期日。
5. 更新星期 `TMP_Text`。
6. 重置每日状态，例如：
   - `DailyMapStepController.ClearActionCounter()`
   - 清理预览
   - 根据需求决定是否刷新卡牌
7. 采集下一天初始状态，保存为新一天快照。
8. 写入 `daily_week_save.json`。

这里建议“进入下一天后也保存一次”，这样存档按钮里能直接看到新一天。

## 星期 TMP_Text 更新

总控脚本中配置：

```csharp
[SerializeField] private TMP_Text weekText;
```

刷新方法：

```csharp
private void RefreshWeekText()
{
    if (weekText == null)
    {
        return;
    }

    int index = Mathf.Clamp(currentDayIndex, 0, weekNames.Length - 1);
    weekText.text = weekNames[index];
}
```

如果 UI 要显示 `第1天 星期一`，可以改成：

```csharp
weekText.text = $"第{index + 1}天 {weekNames[index]}";
```

## 存档按钮逻辑

按钮：

```text
Btn_SaveList
```

绑定方法：

```csharp
public void ShowSaveList()
```

执行顺序：

1. 从 `daily_week_save.json` 读取 `DailyWeekSaveData`。
2. 清空存档列表 UI。
3. 遍历 `days`。
4. 为每个 `DailyDaySaveData` 生成一个按钮。
5. 按钮文本显示：

```text
星期一
星期二
星期三
```

或：

```text
第1天 星期一
```

6. 点击某个按钮时调用：

```csharp
public void LoadDay(int dayIndex)
```

## 回档逻辑

选择某一天后：

```csharp
public void LoadDay(int dayIndex)
```

执行顺序：

1. 如果正在移动，禁止回档。
2. 从 `DailyWeekSaveData.days` 找到对应 `dayIndex` 的快照。
3. 应用 `DailySaveData` 到当前 Daily 系统：
   - 先恢复节点揭示状态
   - 再恢复当前节点和行动次数
   - 再恢复任务进度
   - 最后恢复卡牌
4. 设置 `currentDayIndex = dayIndex`。
5. 更新星期 `TMP_Text`。
6. 删除所有 `dayIndex` 大于当前选择日期的快照。
7. 写回 `daily_week_save.json`。
8. 关闭存档列表 UI。

关键点：

```csharp
weekSave.days.RemoveAll(day => day.dayIndex > dayIndex);
```

这样回到星期三后，星期四、星期五、星期六、星期日的数据都会被删除。

## DailySaveData 的采集和应用

建议新增一个独立脚本：

```csharp
DailySaveController
```

负责把当前场景里的 Daily 状态转成 `DailySaveData`，也负责把 `DailySaveData` 应用回场景。

接口建议：

```csharp
public DailySaveData Capture();
public void Apply(DailySaveData data);
```

`DailyWeekSaveController` 不直接操作每个节点和卡牌细节，只调用：

```csharp
DailySaveData daily = dailySaveController.Capture();
dailySaveController.Apply(daySave.daily);
```

这样职责更干净。

## 需要各模块暴露的接口

`DailyMissionPanelController`：

```csharp
public DailyMissionSaveData CaptureSaveData();
public void ApplySaveData(DailyMissionSaveData data);
```

`DailyMapStepController`：

```csharp
public DailyStepSaveData CaptureSaveData();
public void ApplySaveData(DailyStepSaveData data, IReadOnlyList<DailyMapNode> nodes);
public bool CanChangeDayOrLoadSave();
```

`DailyMapNode`：

```csharp
public DailyNodeSaveData CaptureSaveData();
public void ApplySaveData(DailyNodeSaveData data);
```

`DailyStepCardHand`：

```csharp
public DailyCardHandSaveData CaptureSaveData();
public void ApplySaveData(DailyCardHandSaveData data);
```

恢复卡牌时建议瞬间恢复，不播放 DOTween 动画。

## 加载时不要触发事件

回档时不要调用：

```csharp
DailyMapNode.TriggerStayEvent()
DailyMapNode.Arrive()
DailyNodeActionDispatcher.DispatchNodeAction()
```

原因：

- 会重复发奖励。
- 会重复推进任务。
- 会弹出到达节点面板。
- 会改变玩家本来保存时的状态。

回档只做状态恢复。

## 初始化流程

进入 Daily 场景时：

1. `DailyWeekSaveController.Start()` 检查是否存在 `daily_week_save.json`。
2. 如果不存在：
   - `currentDayIndex = 0`
   - 显示星期一
   - 采集当前初始 Daily 状态
   - 保存为第一天
3. 如果存在：
   - 读取 `DailyWeekSaveData`
   - 加载 `currentDayIndex` 对应的当天数据
   - 刷新星期文本

## UI 结构建议

```text
DailyOperationMap
├── Btn_NextDay
├── Btn_SaveList
├── WeekText
└── SaveListPanel
    └── SaveButtonRoot
        ├── SaveButtonPrefab
        ├── SaveButtonPrefab(Clone)
        └── SaveButtonPrefab(Clone)
```

`SaveListPanel` 打开时应遮挡下层 Daily UI 点击，避免回档按钮和地图按钮同时响应。

## 推荐实现顺序

1. 在 `DailySaveData.cs` 中新增 `DailyWeekSaveData` 和 `DailyDaySaveData`。
2. 新增 `DailySaveController`，先实现 Daily 当前状态采集和应用。
3. 给 `DailyMissionPanelController`、`DailyMapStepController`、`DailyMapNode`、`DailyStepCardHand` 补导出和导入接口。
4. 新增 `DailyWeekSaveController`。
5. 在 Daily 场景挂载：
   - `DailySaveController`
   - `DailyWeekSaveController`
6. 绑定：
   - 下一天按钮
   - 存档列表按钮
   - 星期 TMP_Text
   - 存档列表面板
7. 测试：
   - 星期一进入星期二后保存
   - 星期二行动后保存
   - 打开存档列表能看到星期一、星期二
   - 回档星期一后，星期二数据被删除
   - 回档不会触发节点奖励和弹窗

