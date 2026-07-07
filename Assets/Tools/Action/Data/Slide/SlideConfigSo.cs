using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Tools.Action.Data.Slide
{
    [CreateAssetMenu(menuName = "Action/SlideConfigSo")]
    public class SlideConfigSo:ScriptableObject
    {
        public int _maxSlides;          // 最大连续滑行次数
        public float _windowTime;       // 统计窗口（秒）
        public float _lockDuration;     // 封禁时长（秒）
        
        public float _judgeTime;        // 判断时间（秒）
        [Header("Perfect Dodge / Witch Time")]
        public bool enablePerfectDodgeWitchTime = true;
        public float witchTimeDuration = 1.2f;
        public float witchSlowScale = 0.1f;
        public float witchSelfScale = 1f;
        public float witchCooldown = 3f;
        public TimeScaleType witchAffectedType = TimeScaleType.All;
    }
}
