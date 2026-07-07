using UnityEngine;

namespace Tools.DesignMode
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static bool HasInstance => _instance != null;

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // 1. 尝试在场景中寻找已有对象
                        _instance = FindFirstObjectByType<T>();

                        // 2. 如果场景中没有，则自动创建一个
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject(typeof(T).Name);
                            _instance = singletonObject.AddComponent<T>();
                        
                            // 保证切换场景时不被销毁
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected void ReleaseInstanceReference()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
