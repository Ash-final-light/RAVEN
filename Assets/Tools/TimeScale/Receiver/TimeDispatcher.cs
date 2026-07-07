using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using TimeScale;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.TimeScale.Receiver
{
    public abstract class TimeScaleDispatcher : MonoBehaviour, ITimeScaleDispatcher
    {
        protected string _cachedInstanceId;
        public string InstanceId => _cachedInstanceId;
        public string Tag => gameObject.tag;

        [ShowInInspector]private readonly Dictionary<TimeScaleType, List<ITimeScalable>> _categoryMap = new();
        /// <summary>
        /// 不同组件对应的缩放时间
        /// </summary>
        /// <returns></returns>
        private readonly Dictionary<string,float> _scaleMap = new();
        
        protected void InitCategoryMap()
        {
            foreach (TimeScaleType type in Enum.GetValues(typeof(TimeScaleType)))
            {
                if (type != TimeScaleType.None && type != TimeScaleType.All)
                    _categoryMap[type] = new List<ITimeScalable>();
            }
        }

        protected void CollectScalableComponents(object target)
        {
            if (target == null) return;

            var type = target.GetType();
            // 扫描宿主实例中的所有属性
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<TimeScalableAttribute>();
                if (attr == null) continue;

                // 重点：从 target (即宿主脚本实例) 获取属性的运行值
                var value = prop.GetValue(target);

                if (value is ITimeScalable scalable)
                {
                    if (_categoryMap.TryGetValue(attr.Category, out var list))
                    {
                        list.Add(scalable);
                        _scaleMap[scalable.Id] = 1f;
                        Debug.Log($"[Dispatcher] 成功从宿主 {target} 绑定组件: {prop.Name}");
                    }
                }
            }
        }

        public void ApplyScale(TimeScaleType type, float scale)
        {
            if (_categoryMap.TryGetValue(type, out var scalables))
            {
                foreach (var item in scalables)
                {
                    if(Mathf.Approximately(_scaleMap[item.Id], scale))
                        continue;
                    _scaleMap[item.Id] = scale;
                    item.TimeScaleChanged(scale, type);
                }
            }
        }

        public void RegisterInTimeScaleManager()
        {
            TimeScaleManager.Instance.Register(this);
        }
    }
}