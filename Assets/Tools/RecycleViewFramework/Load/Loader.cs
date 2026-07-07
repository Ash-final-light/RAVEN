using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Tools.RecycleViewFramework.Load
{
    public class Loader<T> : ILoader where T : Object
    {
        protected List<string> keys;
        protected Dictionary<string, T> data;

        public Loader(List<string> keys)
        {
            this.keys = keys;
            this.data = new();

            Load();
        }

        public Dictionary<string, T> Data { get => data; private set => data = value; }

        public virtual void Load()
        {
            Addressables.LoadAssetsAsync<T>(keys, t => { data[t.name] = t; }, Addressables.MergeMode.Union);
        }

        public virtual void Clear()
        {
            keys.Clear();
            data.Clear();
        }
    }
}