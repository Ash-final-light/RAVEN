using System;
using UnityEngine;

namespace Tools.Profile.Data
{
    [Serializable]
    public enum ProfileAction
    {
        /// <summary>
        /// 移动
        /// </summary>
        Move,
        /// <summary>
        /// 震动
        /// </summary>
        Shake,
        /// <summary>
        /// 隐藏
        /// </summary>
        Hide,
        /// <summary>
        /// 显示
        /// </summary>
        Show,
        /// <summary>
        /// 点亮并放大
        /// </summary>
        PopUp,
        /// <summary>
        /// 变黑并缩小
        /// </summary>
        ShutDown
    }
    [Serializable]
    public class ProfileActionValue
    {
        public string key;
        public ProfileAction profileAction;
        public Vector2 startPos;
        public Vector2 endPos;
        public float startScale;
        public float endScale;
        public float duration;
        public bool matchValueProperty;
    }
}