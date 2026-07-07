using System;
using System.Collections.Generic;
using System.Linq;
using Tools.RecycleViewFramework.Load;
using UnityEngine;
using UnityEngine.U2D;
using VContainer;

namespace Tools.RecycleViewFramework.Sprite
{
    public class SpriteManager : IResource
    {
        private Dictionary<string, SpriteAtlas> spriteAtlasDict;

        [Inject]
        public SpriteManager(Dictionary<string, SpriteAtlas> spriteAtlasDict)
        {
            
            foreach (SpriteAtlas atlas in spriteAtlasDict.Values)
            {
                Debug.Log($"SpriteManager Construct {atlas.name}");
            }
            this.spriteAtlasDict = spriteAtlasDict;
        }

        /// <summary>
        /// 获取图集
        /// </summary>
        /// <param name="atlasName">图集名</param>
        /// <returns></returns>
        public SpriteAtlas GetSpriteAtlas(string atlasName)
        {
            return spriteAtlasDict[atlasName];
        }

        /// <summary>
        /// 获取图片，通过遍历所有图集来查找
        /// </summary>
        /// <param name="thumbnail">图集名,图片名</param>
        /// <returns></returns>
        public UnityEngine.Sprite GetSprite(string thumbnail)
        {
            Debug.Log($"SpriteManager GetSprite {thumbnail}");
            UnityEngine.Sprite sprite = null;
            List<string> atlasNames = ToStringList(thumbnail);
            string altasName = atlasNames[0];
            SpriteAtlas atlas = spriteAtlasDict[altasName];
            string spriteName = atlasNames[1];
            
            sprite = atlas.GetSprite(spriteName);
            
            if (sprite == null)
            {
                Debug.LogError($"SpriteManager GetSprite {spriteName} not found in {altasName}");
                
            }
            return sprite;
            
            List<string> ToStringList(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return new List<string>();

                return input
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }
        }
        /// <summary>
        /// 获取指定图集中的图片
        /// </summary>
        /// <param name="atlasName">图集名</param>
        /// <param name="spriteName">图片名</param>
        /// <returns></returns>
        public UnityEngine.Sprite GetSprite(string atlasName, string spriteName)
        {
            SpriteAtlas atlas = spriteAtlasDict[atlasName];
            if (atlas == null) return null;

            return atlas.GetSprite(spriteName);
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }
    }
}
