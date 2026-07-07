using System;
using Tools.Buff;
using UnityEngine;

namespace Tools.TimeScale
{
    /// <summary>
    /// 这里用来更新和时间有关的数据
    /// BuffManager的buff时间更新
    /// </summary>
    public class TimeUpdate:MonoBehaviour
    {
        public float _buffUpdateTime = 1f;
        private BuffManager _buffManager =  new BuffManager();

        private void Start()
        {
            _buffManager.Init();
            InvokeRepeating(nameof(UpdateBuff), 0f, _buffUpdateTime);
        }
        private void UpdateBuff()
        {
            _buffManager.UpdateBuff(_buffUpdateTime);
        }
    }
}