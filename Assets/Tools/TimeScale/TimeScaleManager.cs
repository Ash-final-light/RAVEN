using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TimeScale.Command;
using Tools.DesignMode;
using Tools.TimeScale;
using Tools.TimeScale.Command;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace TimeScale
{
    public class TimeScaleManager:MonoSingleton<TimeScaleManager>
    {
        private readonly TimeScaleMode<TimeScaleGlobalCommand> _globalMode = new();
        private readonly TimeScaleMode<BaseTimeScaleCommand> _singleMode = new();
        private readonly TimeScaleMode<TimeScaleWitchCommand> _witchMode = new();
        // ... 其他 Mode

        private readonly Dictionary<string, ITimeScaleDispatcher> _dispatchers = new();
        

        /// <summary>
        /// 每帧更新时间缩放,TODO:后续交给全局的时间管理器来决定
        /// </summary>
        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            _globalMode.Update(deltaTime);
            _singleMode.Update(deltaTime);
            _witchMode.Update(deltaTime);

            foreach (var dispatcher in _dispatchers.Values)
            {
                // 对每个细分类型进行合并计算
                foreach (TimeScaleType type in Enum.GetValues(typeof(TimeScaleType)))
                {
                    if (type == TimeScaleType.None || type == TimeScaleType.All) continue;

                    float finalScale = CalculateFinalScale(dispatcher.InstanceId, type);
                    dispatcher.ApplyScale(type, finalScale);
                }
            }
        }
        public void AddCommand(BaseTimeScaleCommand command)
        {
            switch (command)
            {
                case TimeScaleWitchCommand timeScaleWitchCommand:
                    _witchMode.AddCommand(timeScaleWitchCommand);
                    break;
                case TimeScaleGlobalCommand timeScaleGlobalCommand:
                    _globalMode.AddCommand(timeScaleGlobalCommand);
                    break;
                case TimeScaleSingleCommand timeScaleSingleCommand:
                    _singleMode.AddCommand(timeScaleSingleCommand);
                    break;
               default:
                   throw new ArgumentOutOfRangeException(nameof(command), command, null);
                   break;
            }
        }

        public void RemoveCommand(string commandId)
        {
            _globalMode.RemoveCommand(commandId);
            _witchMode.RemoveCommand(commandId);
            _singleMode.RemoveCommand(commandId);
            
        }
        private float CalculateFinalScale(string id, TimeScaleType type)
        {
            // TODO: Skill 优先，否则乘积
            //if (_skillMode.IsActive()) return _skillMode.GetTimeScale(id, type);
            
            return _globalMode.GetTimeScale(id, type) * _witchMode.GetTimeScale(id, type) * _singleMode.GetTimeScale(id, type);
        }

        public void Register(ITimeScaleDispatcher d)
        {
            _dispatchers[d.InstanceId] = d;
            _globalMode.AddTarget(d);
            _singleMode.AddTarget(d);
            _witchMode.AddTarget(d);
        }
    }
}
