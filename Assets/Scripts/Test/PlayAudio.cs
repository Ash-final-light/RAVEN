using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Test
{
    public class PlayAudio:MonoBehaviour
    {
        [Header("Settings")]
        public AudioClip hitClip;
        public int poolSize = 10;
    
        // 预留给音频引擎的最小响应时间（秒）
        // 0.005s - 0.01s 通常足以让底层缓冲区完成调度
        public double schedulingDelay = 0.005; 

        private List<AudioSource> _pool = new List<AudioSource>();
        private int _currentIndex = 0;

        void Awake()
        {
            // 1. 初始化音频池，防止播放时动态生成导致的 GC 卡顿
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = new GameObject($"HitSource_{i}");
                obj.transform.SetParent(transform);
                AudioSource source = obj.AddComponent<AudioSource>();
            
                // 关键设置
                source.clip = hitClip;
                source.playOnAwake = false;
                // 必须提前加载到内存
                source.clip.LoadAudioData(); 
            
                _pool.Add(source);
            }
        }

        /// <summary>
        /// 当玩家按下按键时调用此方法
        /// </summary>
        public void OnPlayerHit()
        {
            // 2. 获取当前的 DSP 时间
            double currentDspTime = AudioSettings.dspTime;

            // 3. 计算预定的播放时间：现在 + 极小的偏移
            // 这样可以跳过逻辑层的抖动，直接由声卡时钟准时触发
            double scheduledTime = currentDspTime + schedulingDelay;

            // 4. 从池中取出一个 AudioSource 播放
            AudioSource source = _pool[_currentIndex];
        
            // 核心：使用 PlayScheduled 而不是 Play
            source.PlayScheduled(scheduledTime);
            source.Play();

            // 5. 轮换索引
            _currentIndex = (_currentIndex + 1) % poolSize;
        }

        void Update()
        {
            // 模拟玩家按键（在实际音游中，这应该在 Input 检测中）
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnPlayerHit();
            }
        }
    }
}