using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Action.Data
{
    [CreateAssetMenu(menuName = "Action/Action Clip Container")]
    public class ActionClipContainerSo : SerializedScriptableObject
    {
        [SerializeField] public Dictionary<string, ActionClip> ActionClips;
    }
}