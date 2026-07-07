using System;
using System.Reflection;
using UnityEngine;

namespace Framework.Core.Singleton
{
    /// <summary>
    /// 单例对象的创建工厂
    /// </summary>
    internal static class SingletonCreator
    {
        /// <summary>
        /// 创建普通C#类的单例对象
        /// </summary>
        /// <typeparam name="T">实际单例类型</typeparam>
        /// <returns>单例对象</returns>
        /// <exception cref="Exception">异常</exception>
        internal static T CreateSingleton<T>() where T : class, ISingleton
        {
            var type = typeof(T);
            var monoBehaviourType = typeof(MonoBehaviour);

            if (monoBehaviourType.IsAssignableFrom(type))
            {
                throw new Exception("T is assignable from MonoBehaviour, so can't invoke CreateSingleton<T>() method");
            }

            var instance = CreateNonPublicConstructorObject<T>();
            instance.OnSingletonInit();
            
            return instance;
        }

        /// <summary>
        /// 创建MonoBehaviour的单例对象
        /// </summary>
        /// <param name="isGlobal">是否为全局单例（即过场景不删除）</param>
        /// <typeparam name="T">实际单例类型</typeparam>
        /// <returns>单例对象</returns>
        internal static T CreateMonoSingleton<T>(bool isGlobal) where T : MonoBehaviour, ISingleton
        {
            T instance = null;
            var type = typeof(T);

            // 在非运行模式下不会创建MonoBehaviour单例物体，防止影响场景
            if (!Application.isPlaying)
            {
                return instance;
            }

            // 判断当前场景中是否存在T实例，已存在则直接调用初始化函数
            instance = UnityEngine.Object.FindObjectOfType(type) as T;
            if (instance != null)
            {
                return instance;
            }

            // 创建对应游戏物体并绑定，注意，创建一定要保证为根物体
            var obj = new GameObject(typeof(T).Name);
            if (isGlobal)
            {
                UnityEngine.Object.DontDestroyOnLoad(obj.transform);
            }

            instance = obj.AddComponent(typeof(T)) as T;
            instance!.OnSingletonInit();
            if (isGlobal)
            {
                Debug.Log($"Singleton Global Mono Init: " + typeof(T).Name);
            }
            else
            {
                Debug.Log($"Singleton Mono Init: " + typeof(T).Name);
            }

            return instance;
        }

        /// <summary>
        /// 创建ScriptableObject的单例对象，注意，需要文件处于Resources文件夹下，如果不存在则自行创建新的文件对象
        /// </summary>
        /// <typeparam name="T">实际单例类型</typeparam>
        /// <returns>单例对象</returns>
        internal static T CreateScriptableObjectSingleton<T>() where T : ScriptableObject, ISingleton
        {
            Debug.Log("Singleton ScriptableObject Create: " + typeof(T).Name);
            var instances = Resources.LoadAll<T>("");
            T instance;
            if (instances == null || instances.Length == 0)
            {
                instance = ScriptableObject.CreateInstance<T>();
            }
            else
            {
                instance = instances[0];
            }

            instance.OnSingletonInit();
            Debug.Log("Singleton ScriptableObject Init: " + typeof(T).Name);

            return instance;
        }

        private static T CreateNonPublicConstructorObject<T>() where T : class
        {
            var type = typeof(T);
            var constructorInfo = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null
            );
            if (constructorInfo == null)
            {
                throw new Exception("Can't find Non-Public Constructor() in " + type);
            }

            return constructorInfo.Invoke(null) as T;
        }
    }
}