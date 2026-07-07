using System;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.TimeScale.Receiver
{
    public class CharacterTimeScaleDispatcher : TimeScaleDispatcher
    {
        // 核心：引用持有具体组件和ID的宿主脚本
        [SerializeField] private ActionStateController hostCharacter;
        
        public void Initialize(ActionStateController host)
        {
            hostCharacter = host;
            // 从宿主脚本的 Parameters 中获取 ID，而不是 GameObject 的 ID
            _cachedInstanceId = hostCharacter.characterId;

            InitCategoryMap();
            // 关键：反射的目标是 hostCharacter 实例
            CollectScalableComponents(hostCharacter);
            
            // 注册到管理器
            RegisterInTimeScaleManager();
        }

    }
}