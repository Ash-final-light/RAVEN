using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Common.Resource;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace UIFramework.Resource
{
    /// <summary>
    /// UI 图片资源管理器，负责缓存通过 Addressables 加载的 Sprite.
    /// </summary>
    public class UIImageManager
    {
        private readonly AddressablesManager _addressablesManager;

        private readonly Dictionary<string, Sprite> _sprites = new();
        private readonly Dictionary<string, Dictionary<string, Sprite>> _groupSprites = new();
        private readonly Dictionary<string, object> _singleSpriteKeys = new();
        private readonly Dictionary<string, List<object>> _groupKeys = new();

        public UIImageManager(AddressablesManager addressablesManager)
        {
            _addressablesManager = addressablesManager;
        }

        /// <summary>
        /// 异步加载单张 Sprite，并按资源名缓存.
        /// </summary>
        /// <param name="key">Addressables key.</param>
        /// <param name="callback">加载成功回调.</param>
        public void LoadSpriteAsync(object key, UnityAction<Sprite> callback = null)
        {
            if (key == null)
            {
                Debug.LogError($"{nameof(UIImageManager)} 单图加载失败: key 不能为空.");
                return;
            }

            _addressablesManager.LoadAssetAsync<Sprite>(key, sprite =>
            {
                if (sprite == null)
                {
                    Debug.LogWarning($"{nameof(UIImageManager)} 单图加载返回空资源: {key}.");
                    return;
                }

                _sprites[sprite.name] = sprite;
                _singleSpriteKeys[sprite.name] = key;
                callback?.Invoke(sprite);
            });
        }

        /// <summary>
        /// 异步加载一组 Sprite，并按 groupName 建立分组缓存.
        /// </summary>
        /// <param name="groupName">逻辑分组名.</param>
        /// <param name="keys">Addressables keys.</param>
        /// <param name="callback">每张图片加载成功时的回调.</param>
        /// <param name="mode">Addressables 合并模式.</param>
        /// <param name="releaseDependenciesOnFailure">批量加载失败时是否释放依赖.</param>
        public void LoadSpritesAsync(
            string groupName,
            IEnumerable keys,
            UnityAction<Sprite> callback = null,
            Addressables.MergeMode mode = Addressables.MergeMode.Union,
            bool releaseDependenciesOnFailure = true
        )
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                Debug.LogError($"{nameof(UIImageManager)} 分组加载失败: groupName 不能为空.");
                return;
            }

            if (keys == null)
            {
                Debug.LogError($"{nameof(UIImageManager)} 分组加载失败: keys 不能为空.");
                return;
            }

            var keyList = ToKeyList(keys);
            if (keyList.Count == 0)
            {
                Debug.LogWarning($"{nameof(UIImageManager)} 分组加载跳过: {groupName} 没有可用 key.");
                return;
            }

            if (!_groupSprites.TryGetValue(groupName, out var groupSpriteDict))
            {
                groupSpriteDict = new Dictionary<string, Sprite>();
                _groupSprites[groupName] = groupSpriteDict;
            }

            _groupKeys[groupName] = keyList;

            _addressablesManager.LoadAssetsAsync<Sprite>(
                keyList,
                sprite =>
                {
                    if (sprite == null)
                    {
                        return;
                    }

                    _sprites[sprite.name] = sprite;
                    groupSpriteDict[sprite.name] = sprite;
                    callback?.Invoke(sprite);
                },
                mode,
                releaseDependenciesOnFailure
            );
        }

        /// <summary>
        /// 按图片名获取缓存 Sprite，会遍历所有已缓存图片.
        /// </summary>
        public Sprite GetSprite(string spriteName)
        {
            if (string.IsNullOrWhiteSpace(spriteName))
            {
                return null;
            }

            _sprites.TryGetValue(spriteName, out var sprite);
            return sprite;
        }

        /// <summary>
        /// 按分组和图片名获取缓存 Sprite.
        /// </summary>
        public Sprite GetSprite(string spriteName, string groupName)
        {
            if (string.IsNullOrWhiteSpace(spriteName) || string.IsNullOrWhiteSpace(groupName))
            {
                return null;
            }

            if (!_groupSprites.TryGetValue(groupName, out var groupSpriteDict))
            {
                return null;
            }

            groupSpriteDict.TryGetValue(spriteName, out var sprite);
            return sprite;
        }

        /// <summary>
        /// 释放单张图片的 Addressables 引用，并移除缓存.
        /// </summary>
        public void ReleaseSprite(string spriteName, UnityAction callback = null)
        {
            if (string.IsNullOrWhiteSpace(spriteName))
            {
                callback?.Invoke();
                return;
            }

            _sprites.Remove(spriteName);

            foreach (var groupSpriteDict in _groupSprites.Values)
            {
                groupSpriteDict.Remove(spriteName);
            }

            if (_singleSpriteKeys.TryGetValue(spriteName, out var key))
            {
                _singleSpriteKeys.Remove(spriteName);
                _addressablesManager.ReleaseAsset<Sprite>(key, callback);
                return;
            }

            callback?.Invoke();
        }

        /// <summary>
        /// 释放整个分组的 Addressables 引用，并移除分组缓存.
        /// </summary>
        public void ReleaseGroup(string groupName, UnityAction callback = null)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                callback?.Invoke();
                return;
            }

            if (_groupSprites.TryGetValue(groupName, out var groupSpriteDict))
            {
                foreach (var pair in groupSpriteDict)
                {
                    _sprites.Remove(pair.Key);
                }

                _groupSprites.Remove(groupName);
            }

            if (_groupKeys.TryGetValue(groupName, out var keys))
            {
                _groupKeys.Remove(groupName);
                _addressablesManager.ReleaseAssets<Sprite>(keys, callback);
                return;
            }

            callback?.Invoke();
        }

        /// <summary>
        /// 清空缓存索引，实际资源释放交给外层 AddressablesManager.ClearAllAssets().
        /// </summary>
        public void Clear()
        {
            _sprites.Clear();
            _groupSprites.Clear();
            _singleSpriteKeys.Clear();
            _groupKeys.Clear();
        }

        private static List<object> ToKeyList(IEnumerable keys)
        {
            var keyList = new List<object>();
            foreach (var key in keys)
            {
                if (key != null)
                {
                    keyList.Add(key);
                }
            }

            return keyList;
        }
    }
}
