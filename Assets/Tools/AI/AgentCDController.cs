using System.Collections.Generic;
using UnityEngine;

namespace Tools.AI
{
    public class AgentCDController
    {
        private AgentAbilityConfig config;
    
        // 运行时存储：Key 为动作名，Value 为该动作下次可执行的时间戳
        private Dictionary<string, float> _nextReadyTimes = new Dictionary<string, float>();
        private Dictionary<string, float> _cooldownSettings;

        private float _timer = 0f;

        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
        }
        
        public AgentCDController(AgentAbilityConfig config)
        {
            this.config = config;
            _cooldownSettings = new Dictionary<string, float>();
            foreach(var ability in config.abilities)
            {
                _cooldownSettings[ability.actionName] = ability.cooldown;
            }
        }
        public bool CanExecute(string actionName)
        {
            // 如果没有配置该动作，默认允许执行（或根据需求报错）
            if (!_cooldownSettings.ContainsKey(actionName)) return true;

            // 检查当前时间是否超过了下次就绪时间
            float readyTime = _nextReadyTimes.GetValueOrDefault(actionName, 0f);
            return _timer >= readyTime;
        }

        public void UpdateCooldown(string actionName)
        {
            if (_cooldownSettings.TryGetValue(actionName, out float cd))
            {
                _nextReadyTimes[actionName] = _timer + cd;
            }
        }
    }
}