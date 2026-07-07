using System;
using UnityEngine;

namespace Framework.Core.Singleton
{
    /// <summary>
    /// 针对MonoBehaviour的单场景单例父类
    /// </summary>
    /// <typeparam name="T">实际单例类型</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>
    {
        private static T _instance;

        public static bool HasInstance => _instance != null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = SingletonCreator.CreateMonoSingleton<T>(false);
                }

                return _instance;
            }
        }

        public static bool TryGetInstance(out T instance)
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(T)) as T;

            instance = _instance;
            return instance != null;
        }
        
        protected virtual void OnDestroy()
        {
            Dispose();
        }

        protected void ReleaseInstanceReference()
        {
            if (_instance == this)
                _instance = null;
        }

        public virtual void OnSingletonInit()
        {
            if (_instance != null)
            {
                throw new Exception("Can't invoke OnSingletonInit() when instance is not null");
            }
        }

        public virtual void Dispose()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }

            _instance = null;
        }
    }
}
