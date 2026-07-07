using System;
using System.Collections.Generic;
using System.Linq;
using TimeScale;
using Tools.TimeScale.Command;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Tools.TimeScale
{
    public class TimeScaleMode<T> : ITimeScaleEdit where T : BaseTimeScaleCommand
    {
        private readonly Dictionary<string, T> _commands = new();
        private readonly Dictionary<string, ITimeScaleDispatcher> _targets = new();
        
        // 存储结构：[实例ID][时间类型] = 缩放值
        private readonly Dictionary<string, Dictionary<TimeScaleType, float>> _targetTimeScales = new();

        public void Update(float deltaTime)
        {
            if (_commands.Count == 0) return;

            var toRemove = _commands.Values
                .Where(c => (c.Time += deltaTime) >= c.Duration)
                .Select(c => c.Id).ToList();

            if (toRemove.Count > 0)
            {
                toRemove.ForEach(id => _commands.Remove(id));
                UpdateTimeScales();
            }
        }

        public bool IsActive() => _commands.Count != 0;

        // 获取特定目标在特定类型下的缩放
        public float GetTimeScale(string instanceId, TimeScaleType type)
        {
            if (_targetTimeScales.TryGetValue(instanceId, out var typeMap))
            {
                return typeMap.GetValueOrDefault(type, 1f);
            }
            return 1f;
        }

        // 实现 ITimeScaleEdit：设置特定类型的缩放
        public float GetTimeScale(ITimeScaleDispatcher target, TimeScaleType type)
        {
            return GetTimeScale(target.InstanceId, type);
        }

        public void SetTimeScale(ITimeScaleDispatcher target, TimeScaleType type, float scale)
        {
            if (!_targetTimeScales.ContainsKey(target.InstanceId)) return;
            
            // 遍历枚举，将命令涉及到的位都设置上
            foreach (TimeScaleType t in Enum.GetValues(typeof(TimeScaleType)))
            {
                if (t != TimeScaleType.None && t != TimeScaleType.All && (type & t) != 0)
                {
                    _targetTimeScales[target.InstanceId][t] = scale;
                }
            }
        }

        public void AddCommand(T command)
        {
            if (_commands.TryAdd(command.Id, command)) UpdateTimeScales();
        }

        public void RemoveCommand(string commandId)
        {
            if (_commands.Remove(commandId)) UpdateTimeScales();
        }

        public void AddTarget(ITimeScaleDispatcher target)
        {
            _targets.TryAdd(target.InstanceId, target);
            _targetTimeScales.TryAdd(target.InstanceId, CreateDefaultTypeMap());
            UpdateTimeScales();
        }

        public void RemoveTarget(ITimeScaleDispatcher target)
        {
            _targets.Remove(target.InstanceId);
            _targetTimeScales.Remove(target.InstanceId);
        }

        private void UpdateTimeScales()
        {
            // 重置所有槽位为 1
            foreach (var typeMap in _targetTimeScales.Values)
            {
                var keys = typeMap.Keys.ToList();
                foreach (var k in keys) typeMap[k] = 1f;
            }

            // 执行所有命令
            foreach (var target in _targets.Values)
            {
                foreach (var command in _commands.Values)
                {
                    command.Execute(this, target);
                }
            }
        }

        private Dictionary<TimeScaleType, float> CreateDefaultTypeMap()
        {
            var map = new Dictionary<TimeScaleType, float>();
            foreach (TimeScaleType t in Enum.GetValues(typeof(TimeScaleType)))
            {
                if (t != TimeScaleType.None && t != TimeScaleType.All) map[t] = 1f;
            }
            return map;
        }
    }
}