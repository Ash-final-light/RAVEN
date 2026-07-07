using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using VContainer.Unity;

namespace Tools.RecycleViewFramework.Load
{
    public class ResourceManager : IStartable
    {
        public Dictionary<string, SpriteAtlas> SpriteAtlas { get; private set; } = new();
        public Dictionary<string, GameObject> Views { get; private set; } = new();
        public Dictionary<string, TextAsset> Jsons { get; private set; } = new();

        public Dictionary<string,AudioClip> Sounds { get; private set; } = new();
        private int resCounter = 0;
        private const int maxCount = 4;

        public event System.Action OnResourceLoaded;

        public void Start()
        {
            LoadSprites();
            LoadViews();
            LoadJsons();
            LoadSounds();
        }

        private void HandleLoadCompleted()
        {
            resCounter++;
            if (resCounter >= maxCount)
            {
                OnResourceLoaded?.Invoke();
            }
        }

        private void LoadSprites()
        {
            Debug.Log("ResourceManager LoadSprites");
            Addressables.LoadAssetsAsync<SpriteAtlas>(
                new List<string> { "Atlas" },
                t => SpriteAtlas[t.name] = t,
                Addressables.MergeMode.Union
            ).Completed += _ => HandleLoadCompleted();
        }

        private void LoadViews()
        {
            Debug.Log("ResourceManager LoadViews");
            Addressables.LoadAssetsAsync<GameObject>(
                new List<string> { "View" },
                t => Views[t.name] = t,
                Addressables.MergeMode.Union
            ).Completed += _ => HandleLoadCompleted();
        }

        private void LoadJsons()
        {
            Debug.Log("ResourceManager LoadJsons");
            Addressables.LoadAssetsAsync<TextAsset>(
                new List<string> { "Json" },
                t => Jsons[t.name] = t,
                Addressables.MergeMode.Union
            ).Completed += _ => HandleLoadCompleted();
        }
        private void LoadSounds()
        {
            Debug.Log("ResourceManager LoadSounds");
            Addressables.LoadAssetsAsync<AudioClip>(
                new List<string> { "Audio" },
                t => Sounds[t.name] = t,
                Addressables.MergeMode.Union
            ).Completed += _ => HandleLoadCompleted();
        }
    }
}