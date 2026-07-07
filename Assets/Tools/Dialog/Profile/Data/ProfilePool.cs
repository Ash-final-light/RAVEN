using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Profile.Data
{
    [CreateAssetMenu()]
    [Serializable]
    public class ProfilePool:ScriptableObject
    {
        /// <summary>
        /// key: sprite name
        /// value: sprite atlas
        /// </summary>
        public List<ProfileValue> sourceDatas = new ();
    }

    [Serializable]
    public class ProfileValue
    {
        public string Name;
        public string spriteName;
        public string atlasName;
    }
}