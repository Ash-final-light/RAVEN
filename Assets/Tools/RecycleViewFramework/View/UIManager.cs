using System.Collections.Generic;
using Tools.RecycleViewFramework.Load;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Tools.RecycleViewFramework.View
{
    public class UIManager : IResource
    {
        private List<View> views;
        private Dictionary<string, GameObject> dict = new();

        private readonly IObjectResolver resolver;
        private readonly ResourceManager resourceManager;

        public System.Action OnLoaded;
        public Canvas GameUI { get; private set; }
        public bool ResourcesLoaded { get; private set; }

        [Inject]
        public UIManager(IObjectResolver resolver, ResourceManager resourceManager)
        {
            Debug.Log("UIManager Construct");

            this.resolver = resolver;
            this.resourceManager = resourceManager;

            views = new();
            GameUI = FindGameUICanvas();
            if(GameUI == null)
            {
                Debug.LogError("UIManager GameUI is null");
                return;
            }

            resourceManager.OnResourceLoaded += OnResourceLoaded;
        }

        private static Canvas FindGameUICanvas()
        {
            var canvasObject = GameObject.Find("UICanvas") ?? GameObject.Find("BattleHUDCanvas");
            if (canvasObject != null && canvasObject.TryGetComponent(out Canvas namedCanvas))
            {
                return namedCanvas;
            }

#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<Canvas>();
#else
            return Object.FindObjectOfType<Canvas>();
#endif
        }

        private void OnResourceLoaded()
        {
            dict = resourceManager.Views;
            ResourcesLoaded = true;
            OnLoaded?.Invoke();
        }

        public bool HasViewResource(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                   dict != null &&
                   dict.ContainsKey(name);
        }

        // =================== 原方法全部保留 ===================

        public void Toggle(string name)
        {
            if (TryGetView(name, out View view))
            {
                if (view.Visibility) StopView(view);
                else StartView(view);
            }
        }
        /// <summary>
        /// 打开 View，并获取 view 对象
        /// </summary>
        /// <param name="name">View名称</param>
        /// <param name="view">获取的 view 对象</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Open<T>(string name, out T view) where T : View
        {
            if (TryGetView(name, out view))
            {
                if (!view.Visibility)
                {
                    StartView(view);
                }
                return true;
            }
            if (CreateView(name, out View v))
            {
                view = v as T;
                StartView(view);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 打开 View
        /// </summary>
        /// <param name="name">View名称</param>
        public void Open(string name)
        {
            // 日志1：打印调用信息和字典状态
            Debug.Log($"UIManager Open 调用，名称：{name}，字典总数：{dict?.Count ?? 0}");
    
            // 补充字典空值校验
            if (dict == null)
            {
                Debug.LogError($"UIManager Open {name} failed，字典为null");
                return;
            }

            // 检查字典是否包含该名称
            if(!dict.ContainsKey(name))
            {
                Debug.LogError($"UIManager Open {name} failed, not found in dict！字典中的Key列表：{string.Join(",", dict.Keys)}");
                return;
            }

            // 尝试获取已存在的View
            if (TryGetView(name, out View view))
            {
                Debug.Log($"UIManager Open {name}，找到已存在的View：{view.name}，当前可见性：{view.Visibility}");
                if (!view.Visibility)
                {
                    StartView(view);
                    Debug.Log($"UIManager Open {name}，已显示已存在的View");
                }
                return;
            }

            Debug.Log($"UIManager Open {name}，未找到已存在的View，尝试创建");
    
            // 创建新View
            if (CreateView(name, out view))
            {
                Debug.Log($"UIManager Open {name}，成功创建View：{view.name}，启动View");
                StartView(view);
            }
            else
            {
                Debug.LogError($"UIManager Open {name} failed，创建View失败");
            }
        }

        

        public void Close(string name)
        {
            if (TryGetView(name, out View view))
                Close(view);
        }

        public void Close(View view)
        {
            if (view.Visibility)
                StopView(view);
        }

        private bool CreateView(string name, out View view)
        {
            if (!dict[name].TryGetComponent(out View original))
            {
                view = null;
                return false;
            }

            view = UnityEngine.Object.Instantiate(original);
            view.transform.SetParent(GameUI.transform, false);

            // ⭐⭐⭐ 关键注入
            resolver.InjectGameObject(view.gameObject);

            view.Name = name;
            view.OnCreate(this);

            views.Add(view);
            return true;
        }

        /// <summary>
        /// 通过名称获取 View 对象
        /// </summary>
        /// <param name="name">View名称</param>
        /// <param name="view">获取的 view 对象</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetView<T>(string name, out T view) where T : View
        {
            View v = views.Find(v => v.Name == name);
            if (v != null)
            {
                view = v as T;
                return true;
            }

            if (CreateView(name, out v))
            {
                view = v as T;
                return true;
            }
            view = null;
            return false;
        }


        private void StartView(View view)
        {
            view.OnStart();

            if (view.ViewType == ViewType.Main
                || view.ViewType == ViewType.Activity
                || view.ViewType == ViewType.Dialog)
            {
                // Cursor.visible = true;
            }
            int count = views.FindAll(v => v.ViewType == view.ViewType && v.Visibility).Count - 1;
            view.SetSortingOrder(count);
        }


        private void StopView(View view)
        {
            view.OnStop();

            if (view.ViewType == ViewType.Main
                || view.ViewType == ViewType.Activity
                || view.ViewType == ViewType.Dialog)
            {
                // Cursor.visible = HasActivity();
            }
        }


        public void Clear()
        {
            
        }
        /// <summary>
        /// View关闭动画结束之后的操作
        /// </summary>
        /// <param name="view"></param>
        public void HandleClosed(View view)
        {
            if (view.Child != null)
            {
                Open(view.Child);
                view.Child = null;
            }
            else if (!view.Cache)
            {
                view.OnDestroy();
                views.Remove(view);
            }
        }
        /// <summary>
        /// 是否还有可以阻塞的View
        /// </summary>
        /// <returns></returns>
        public bool HasActivity()
        {
            int count = views.FindAll(v => v.ViewType == ViewType.Activity && v.Visibility).Count;
            return count > 0;
        }
        /// <summary>
        /// 关闭最顶层的View，此View必须是可以回退的，CanBack == true
        /// </summary>
        public void CloseTopView()
        {
            if (views.Count == 0) return;

            View topView = null;
            foreach (var view in views)
            {
                if (view.ViewType == ViewType.Activity && view.Visibility)
                {
                    if (topView == null || topView.GetSortingOrder() < view.GetSortingOrder())
                    {
                        topView = view;
                    }
                }
            }
            if (topView != null)
            {
                Close(topView);
            }
        }
        public void Open(string name, View parent)
        {
            parent.Child = name;
            Close(parent);
        }


    }
}
