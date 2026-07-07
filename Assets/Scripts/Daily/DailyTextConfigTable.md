# Daily 文本配置表

配置路径：

```text
Assets/Scripts/Daily/Config
```

当前使用的 SO 类型：

```text
TextDic
```

建议拆成两个配置资产：

```text
PopupTextDic.asset
ExplainTextDic.asset
```

- `PopupTextDic`：角色到达节点后的提示文本，供 `PopupPanel` 使用。
- `ExplainTextDic`：点击节点后的说明文本，供 `ExplainPanel` 使用。

## 可用占位符

当前 `TextDic` 已支持：

| 占位符 | 含义 | 适用节点 |
| --- | --- | --- |
| `{TargetValue}` | 节点配置中的目标数值 | `AdjustStep`、`ExtraAction` |

当前映射规则：

| 节点类型 | `{TargetValue}` 对应字段 |
| --- | --- |
| `AdjustStep` | `node.Properties.AdjustStepAmount` |
| `ExtraAction` | `node.Properties.ExtraActionCount` |

## PopupTextDic 配置表

| 节点类型 | 配置文本建议 | 显示时机 |
| --- | --- | --- |
| `Battle1` | 遭遇1号战斗。 | 角色到达战斗1节点 |
| `Battle2` | 遭遇2号战斗。 | 角色到达战斗2节点 |
| `Battle3` | 遭遇3号战斗。 | 角色到达战斗3节点 |
| `Resource` | 已获得支援箱补给。 | 角色到达支援箱节点 |
| `CrossValidation` | 已完成交叉验证。 | 角色到达交叉验证节点 |
| `AbandonedWarehouse` | 已探索废弃仓库。 | 角色到达废弃仓库节点 |
| `ExtraAction` | 已获得额外行动次数 +{TargetValue}。 | 角色到达额外行动节点 |
| `ExternalSupport` | 已触发外部支援任务。 | 角色到达外部支援节点 |
| `AdjustStep` | 已触发调动步数，将额外移动 {TargetValue} 步。 | 角色到达调动步数节点 |
| `PreciseRemoteSensing` | 已获得精确遥感情报。 | 角色到达精确遥感节点 |

## ExplainTextDic 配置表

| 节点类型 | 配置文本建议 | 显示时机 |
| --- | --- | --- |
| `Battle1` | 战斗1节点。抵达后会打开1号战斗确认。 | 点击战斗1节点 |
| `Battle2` | 战斗2节点。抵达后会打开2号战斗确认。 | 点击战斗2节点 |
| `Battle3` | 战斗3节点。抵达后会打开3号战斗确认。 | 点击战斗3节点 |
| `Resource` | 支援箱节点。抵达后可获得支援箱补给。 | 点击支援箱节点 |
| `CrossValidation` | 交叉验证节点。抵达后完成一次交叉验证任务。 | 点击交叉验证节点 |
| `AbandonedWarehouse` | 废弃仓库节点。抵达后完成一次废弃仓库探索。 | 点击废弃仓库节点 |
| `ExtraAction` | 额外行动节点。抵达后增加行动次数 +{TargetValue}。 | 点击额外行动节点 |
| `ExternalSupport` | 外部支援节点。抵达后触发支援相关任务。 | 点击外部支援节点 |
| `AdjustStep` | 调动步数节点。抵达后不消耗行动次数，额外移动 {TargetValue} 步。 | 点击调动步数节点 |
| `PreciseRemoteSensing` | 精确遥感节点。抵达后获得额外侦查信息。 | 点击精确遥感节点 |

## Inspector 配置方式

在 `TextDic` 的 `Data` 字典中添加以下键值：

```text
DailyMapNodeEventType -> string
```

示例：

| Key | Value |
| --- | --- |
| `AdjustStep` | `已触发调动步数，将额外移动 {TargetValue} 步。` |
| `ExtraAction` | `已获得额外行动次数 +{TargetValue}。` |

## 后续扩展建议

如果需要显示更多节点属性，建议扩展 `TextDic.GetDescription(DailyMapNode node)`，增加更多占位符：

| 新占位符 | 对应字段 |
| --- | --- |
| `{NodeId}` | `node.NodeId` |
| `{EventName}` | `node.Properties.EventName` |
| `{BattleLevel}` | `node.Properties.BattleLevel` |
| `{ResourceAmount}` | `node.Properties.ResourceAmount` |
| `{RemoteSensingCount}` | `node.Properties.RemoteSensingCount` |
| `{ExternalSupportMissionId}` | `node.Properties.ExternalSupportMissionId` |
