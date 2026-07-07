using System;
using Sirenix.OdinInspector;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Tools.Audio
{
    [Serializable]
    public class AudioAbility:ITimeScalable
    {
        [SerializeField, MinValue(0f)] private float soundMinDistance = 1f;
        [SerializeField, MinValue(0f)] private float soundMaxDistance = 10f;
        [SerializeField] private bool debug;

        private GameAudioManager _audioManager;

        public AudioAbility(GameAudioManager audioManager,bool debug = false)
        {
            _audioManager = audioManager;
            this.debug = debug;
        }

        public void Tick(float deltaTime)
        {
        }

        protected void Dispose()
        {
            _audioManager.ClearSounds();
            GameObject.Destroy(_audioManager);
        }

        public int PlaySound(
            AudioClip audioClip,
            bool loop = false,
            float volume = 1f,
            float duration = float.MaxValue
        )
        {
            var id = _audioManager.PlaySound(
                audioClip,
                loop,
                volume,
                duration,
                spatialBlend: 1f,
                minDistance: soundMinDistance,
                maxDistance: soundMaxDistance
            );
            if (debug)
            {
                Debug.Log($"角色()播放音频: {audioClip.name}—{id}");
            }

            return id;
        }

        public void StopSound(int id)
        {
            if (debug)
            {
                Debug.Log($"角色()停止音频: {id}");
            }

            _audioManager.StopSound(id);
        }

        public void StopSounds(AudioClip audioClip)
        {
            if (debug)
            {
                Debug.Log($"角色()停止音频: {audioClip.name}");
            }

            _audioManager.StopSounds(audioClip);
        }

        public void SetSoundSpeed(float speed)
        {
            _audioManager.SetSoundsPitch(speed);
        }
        
        private string id = Guid.NewGuid().ToString();
        public string Id => id;
        public void TimeScaleChanged(float scale, TimeScaleType type)
        {
            if(type == TimeScaleType.Audio)
            {
                _audioManager.SetSoundsPitch(scale);
                //Debug.Log($"[TimeScale Test: AudioAbility] 组件收到缩放: 类型={type}, 值={scale}");
            }
        }
    }
}