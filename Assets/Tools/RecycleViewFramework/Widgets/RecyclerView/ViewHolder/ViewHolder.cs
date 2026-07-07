using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder
{
    /// <summary>
    /// 实现红点系统需要UpdateReddotState(int index)方法
    /// </summary>
    public abstract class ViewHolder : MonoBehaviour
    {
        private bool isStarted;

        private RectTransform rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                {
                    rectTransform = GetComponent<RectTransform>();
                }
                return rectTransform;
            }
            private set
            {
                rectTransform = value;
            }
        }

        public string Name { get; set; }
        public int Index { get; set; }

        public Vector2 SizeDelta => RectTransform.sizeDelta;

        public virtual void OnStart()
        {
            
            if (!isStarted)
            {
                isStarted = true;
                FindUI();
            }
        }

        public virtual void OnStop() { }

        public abstract void FindUI();

        public abstract void BindViewData<T>(T data);

        public virtual void BindItemClick<T>(T data, Action<T> action)
        {
            if (TryGetComponent(out Button button))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => action?.Invoke(data));
            }
        }

        public virtual void BindChoiceState(bool state) { }

        /// <summary>
        /// 显示红点
        /// </summary>
        /// <param name="index"></param>
        public virtual void UpdateReddotState<T>(T data)
        {
            
        }
    }
}
