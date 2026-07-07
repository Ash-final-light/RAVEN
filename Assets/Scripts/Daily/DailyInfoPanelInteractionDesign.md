# Daily 信息面板交互需求与实现方案

## 需求

在 Daily 行动地图中，节点信息会通过三个面板显示：

- `DailyActionInfo`
- `DailyObjectInfo`
- `DailyFightInfo`

当任意一个面板显示时，必须遮挡并拦截其它所有 UI 的点击事件，底层的步数卡、节点、顶部按钮等都不能被点击到。

关闭规则：

- `DailyActionInfo`：点击屏幕任意位置关闭。
- `DailyObjectInfo`：点击屏幕任意位置关闭。
- `DailyFightInfo`：不能点击任意位置关闭，必须点击 `Btn_Avoid` 或 `Btn_Accept`。

该逻辑需要拆成两个脚本实现并挂载，避免所有面板交互都挤在一个脚本里。

## 脚本拆分

### 1. `DailyClickCloseInfoPanel`

负责 `DailyActionInfo` 和 `DailyObjectInfo`。

挂载位置：

- 挂在 `DailyActionInfo`
- 挂在 `DailyObjectInfo`

职责：

- 面板显示时创建或启用全屏点击遮罩。
- 遮罩层级必须高于地图中其它 UI。
- 面板本体层级必须高于遮罩。
- 遮罩拦截所有底层 UI 点击。
- 点击遮罩任意位置时关闭当前面板。
- 为了让“点击面板内容区域也能关闭”，面板内非交互 `Graphic` 的 `raycastTarget` 应关闭，使点击穿到遮罩。

预期字段：

```csharp
[SerializeField] private GameObject panelRoot;
[SerializeField] private bool closeOnAnyScreenClick = true;
```

预期核心方法：

```csharp
public void Show();
public void Hide();
```

行为：

```text
Show()
  -> panelRoot.SetActive(true)
  -> 显示全屏 blocker
  -> blocker.SetAsLastSibling()
  -> panelRoot.SetAsLastSibling()

点击 blocker
  -> Hide()
  -> panelRoot.SetActive(false)
  -> 隐藏 blocker
```

### 2. `DailyFightInfoPanelController`

负责 `DailyFightInfo`。

挂载位置：

- 挂在 `DailyFightInfo`

职责：

- 面板显示时创建或启用全屏点击遮罩。
- 遮罩层级必须高于地图中其它 UI。
- `DailyFightInfo` 面板本体层级必须高于遮罩。
- 遮罩只负责拦截底层点击，不负责关闭面板。
- `Btn_Avoid` 点击后执行回避逻辑并关闭面板。
- `Btn_Accept` 点击后执行接战逻辑并关闭面板。
- 除 `Btn_Avoid` 和 `Btn_Accept` 外，点击屏幕任意区域不关闭面板。

预期字段：

```csharp
[SerializeField] private GameObject panelRoot;
[SerializeField] private Button avoidButton;
[SerializeField] private Button acceptButton;
```

预期核心方法：

```csharp
public void Show(DailyMapNode node);
public void Hide();
private void OnAvoidClicked();
private void OnAcceptClicked();
```

行为：

```text
Show(node)
  -> panelRoot.SetActive(true)
  -> 显示全屏 blocker
  -> blocker.SetAsLastSibling()
  -> panelRoot.SetAsLastSibling()
  -> blocker 不绑定关闭事件

点击 Btn_Avoid
  -> 执行回避逻辑
  -> Hide()

点击 Btn_Accept
  -> 执行接战逻辑
  -> Hide()
```

## 全屏遮罩建议

两个脚本都需要使用同一种全屏遮罩策略。

遮罩对象建议命名：

```text
DailyInfoPanelClickBlocker
```

遮罩结构：

```text
DailyOperationMapCanvas
  DailyInfoPanelClickBlocker
```

组件：

- `RectTransform`
- `CanvasRenderer`
- `Image`
- `Button`，仅 `DailyClickCloseInfoPanel` 需要使用按钮点击关闭

遮罩参数：

```csharp
rect.anchorMin = Vector2.zero;
rect.anchorMax = Vector2.one;
rect.offsetMin = Vector2.zero;
rect.offsetMax = Vector2.zero;
image.color = new Color(0, 0, 0, 0.01f);
image.raycastTarget = true;
```

说明：

- alpha 使用 `0.01f`，保证可接收 UI Raycast。
- 遮罩必须在显示时 `SetAsLastSibling()`。
- 面板随后 `SetAsLastSibling()`，保证面板显示在遮罩之上。
- `DailyFightInfo` 内的 `Btn_Avoid` 和 `Btn_Accept` 必须保持 `raycastTarget` 可交互。

## DailyMapNode 调用方式

节点只负责决定显示哪类面板：

```text
普通点击节点
  -> 显示 DailyObjectInfo
  -> 使用 DailyClickCloseInfoPanel

角色到达非战斗节点
  -> 显示 DailyActionInfo
  -> 使用 DailyClickCloseInfoPanel

角色到达战斗节点
  -> 显示 DailyFightInfo
  -> 使用 DailyFightInfoPanelController
```

战斗节点类型：

- `Battle1`
- `Battle2`
- `Battle3`

非战斗节点类型：

- `Resource`
- `ExtraAction`
- `ExternalSupport`
- `AdjustStep`
- `PreciseRemoteSensing`

## 挂载清单

```text
DailyOperationMapCanvas/DailyActionInfo
  DailyClickCloseInfoPanel

DailyOperationMapCanvas/DailyObjectInfo
  DailyClickCloseInfoPanel

DailyOperationMapCanvas/DailyFightInfo
  DailyFightInfoPanelController

DailyOperationMapCanvas/DailyFightInfo/buttons/Btn_Avoid
  Button

DailyOperationMapCanvas/DailyFightInfo/buttons/Btn_Accept
  Button
```

## 测试点

1. 显示 `DailyActionInfo` 后，点击步数卡区域，步数卡不响应，面板关闭。
2. 显示 `DailyObjectInfo` 后，点击节点区域，节点不响应，面板关闭。
3. 显示 `DailyFightInfo` 后，点击面板外区域，面板不关闭，底层 UI 不响应。
4. 显示 `DailyFightInfo` 后，点击 `Btn_Avoid`，执行回避逻辑并关闭。
5. 显示 `DailyFightInfo` 后，点击 `Btn_Accept`，执行接战逻辑并关闭。
6. 任意面板显示时，UI Raycast 顶层命中对象必须是遮罩或当前面板按钮，不能是底层 UI。
