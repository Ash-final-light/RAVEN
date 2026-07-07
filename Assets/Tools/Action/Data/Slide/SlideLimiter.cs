using System.Collections.Generic;
using UnityEngine;

namespace Tools.Action.Data.Slide
{
    public class SlideLimiter
    {
        private int _maxSlides;          // 最大连续滑行次数
        private float _windowTime;       // 统计窗口（秒）
        private float _lockDuration;     // 封禁时长（秒）

        private List<float> _slideTimeStamps = new(); // 记录每次滑行的时间点
        private float _lockUntilTime = -1f;           // 封禁截止时间

        public SlideLimiter(int maxSlides, float windowTime, float lockDuration)
        {
            _maxSlides = maxSlides;
            _windowTime = windowTime;
            _lockDuration = lockDuration;
        }

        /// <summary>
        /// 检查当前是否允许滑行
        /// </summary>
        public bool CanSlide()
        {
            // 如果正处于封禁期
            if (Time.time < _lockUntilTime) return false;

            // 清理掉超过窗口期的旧记录
            CleanExpiredRecords();

            // 如果记录数未达上限，允许闪避
            return _slideTimeStamps.Count < _maxSlides;
        }

        /// <summary>
        /// 成功执行滑行后调用
        /// </summary>
        public void RecordSlide()
        {
            _slideTimeStamps.Add(Time.time);

            // 再次检查，如果达到上限，立即封禁
            if (_slideTimeStamps.Count >= _maxSlides)
            {
                _lockUntilTime = Time.time + _lockDuration;
                _slideTimeStamps.Clear(); // 封禁后清空记录
            }
        }

        private void CleanExpiredRecords()
        {
            float currentTime = Time.time;
            // 移除所有在 windowTime 之前的记录
            _slideTimeStamps.RemoveAll(t => currentTime - t > _windowTime);
        }
        
        public float GetRemainingLockTime() => Mathf.Max(0, _lockUntilTime - Time.time);
    }
}