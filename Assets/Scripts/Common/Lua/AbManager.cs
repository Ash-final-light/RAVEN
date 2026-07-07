using System;
using System.Collections;
using System.Collections.Generic;
using Tools.DesignMode;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Common
{
    public class AbManager:MonoSingleton<AbManager>
    {
        private AssetBundle mainBundle;
        private AssetBundleManifest manifest;

        // 缓存已加载的 AB 包
        private Dictionary<string, AssetBundle> abDict = new Dictionary<string, AssetBundle>();
        

        string PathURL => Application.streamingAssetsPath + "/ABs";

        string BundleName
        {
            get
            {
#if UNITY_IOS
                return "IOS";
#elif UNITY_ANDROID
                return "ANDROID";
#elif UNITY_STANDALONE_WIN
                return "PC";
#else
                return "PC";
#endif
            }
        }

        // 核心修正：增加检查，避免重复加载
        private void LoadADependency(string abName)
        {
            if (mainBundle == null)
            {
                mainBundle = AssetBundle.LoadFromFile(PathURL + "/" + BundleName);
                if (mainBundle != null)
                    manifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }

            if (manifest == null) return;

            // 1. 处理依赖包
            string[] depends = manifest.GetAllDependencies(abName);
            foreach (var item in depends)
            {
                if (!abDict.ContainsKey(item))
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(PathURL + "/" + item);
                    if (ab != null) abDict.Add(item, ab);
                }
            }

            // 2. 处理目标包
            if (!abDict.ContainsKey(abName))
            {
                AssetBundle ab = AssetBundle.LoadFromFile(PathURL + "/" + abName);
                if (ab != null) abDict.Add(abName, ab);
            }
        }

        // 泛型加载：粒子池最常用的接口
        // 注意：移除了 Instantiate，由调用者决定是否克隆
        public T LoadRes<T>(string abName, string assetName) where T : Object
        {
            LoadADependency(abName);
            if (!abDict.ContainsKey(abName)) return null;
            return abDict[abName].LoadAsset<T>(assetName);
        }

        public void LoadResAsync<T>(string abName, string assetName, UnityAction<T> callback) where T : Object
        {
            // 注意：这里为了严谨，LoadADependency 也应改为异步，
            // 但如果为了快速修复报错，至少在这里先保证逻辑正确。
            LoadADependency(abName);
            if (!abDict.ContainsKey(abName))
            {
                callback?.Invoke(null);
                return;
            }
            StartCoroutine(ReallyLoadResAsync<T>(abName, assetName, callback));
        }

        private IEnumerator ReallyLoadResAsync<T>(string abName, string assetName, UnityAction<T> callback) where T : Object
        {
            AssetBundleRequest request = abDict[abName].LoadAssetAsync<T>(assetName);
            yield return request;
            // 粒子池需要 Prefab 本身，所以这里绝不 Instantiate
            callback?.Invoke(request.asset as T);
        }

        public void UnloadRes(string abName)
        {
            if (abDict.ContainsKey(abName))
            {
                // false 表示只卸载 AB 包镜像，不卸载已从包里加载出来的资源
                abDict[abName].Unload(false);
                abDict.Remove(abName);
            }
        }

        public void UnloadAllRes()
        {
            AssetBundle.UnloadAllAssetBundles(false);
            abDict.Clear();
            manifest = null;
            if (mainBundle != null)
            {
                mainBundle.Unload(true);
                mainBundle = null;
            }
        }
    }
}