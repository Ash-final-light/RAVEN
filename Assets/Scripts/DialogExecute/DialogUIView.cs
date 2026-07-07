using Common;
using System;
using NaiQiu.Framework.View;
using TMPro;
using Tools.Dialog;
using Tools.Dialog.Nodes;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.View;
using Tools.RecycleViewFramework.Widgets.RecyclerView;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Layout;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DialogExecute
{
    public class DialogUIView:View
    {
        public bool debug = true;
        
        private TMP_Text speakerText;
        private TMP_Text contentText;

        private Button _BtnEsc;
        private Button _BtnNext;
        private Button _BtnReview;
        private Button _BtnSkip;
        private Button _BtnAuto;

        private Button _BtnOption1;
        private Button _BtnOption2;
        private Button _BtnOption3;
        
        private TMP_Text _option1Text;
        private TMP_Text _option2Text;
        private TMP_Text _option3Text;
        
        private Adapter<DialogText> dialogAdapter;
        private RecyclerView recyclerView;
        private GameObject optionPool;
        
        [Inject] public readonly DialogTreeExecutor executor;
        [SerializeField] private DialogTree dialogTree;//这里用来演示运行
        
        private Coroutine typingCoroutine;
        private bool isTyping = false;
        private Action onCompleted;
        private bool completionNotified;
        [Range(0, 0.5f)] public float typingSpeed = 0.03f; // 每个字符间隔

        public override void OnCreate(UIManager uiManager)
        {
            base.OnCreate(uiManager);
            speakerText = transform.Find("DialogContent/Speaker").GetComponent<TMP_Text>();
            contentText = transform.Find("DialogContent/Content").GetComponent<TMP_Text>();

            _BtnEsc = transform.Find("ToolBar/BtnEsc").GetComponent<Button>();
            _BtnNext = transform.Find("ToolBar/BtnNext").GetComponent<Button>();
            _BtnSkip = transform.Find("ToolBar/BtnSkip").GetComponent<Button>();
            _BtnAuto = transform.Find("ToolBar/BtnAuto").GetComponent<Button>();
            _BtnReview = transform.Find("ToolBar/BtnReview").GetComponent<Button>();
            
            _BtnOption1 = transform.Find("OptionPool/OptionListView/Content/BtnOption1").GetComponent<Button>();
            _BtnOption2 = transform.Find("OptionPool/OptionListView/Content/BtnOption2").GetComponent<Button>();
            _BtnOption3 = transform.Find("OptionPool/OptionListView/Content/BtnOption3").GetComponent<Button>();
            
            _option1Text = _BtnOption1.GetComponentInChildren<TMP_Text>();
            _option2Text = _BtnOption2.GetComponentInChildren<TMP_Text>();
            _option3Text = _BtnOption3.GetComponentInChildren<TMP_Text>();
            
            recyclerView = transform.Find("ReviewList/ReviewListView").GetComponent<RecyclerView>();
            optionPool = transform.Find("OptionPool").gameObject;
            dialogAdapter = new Adapter<DialogText>(recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager());

            _BtnReview.onClick.AddListener(HandleShowReview);
            _BtnAuto.onClick.AddListener(HandleAuto);
            _BtnNext.onClick.AddListener(HandleNext);
            _BtnSkip.onClick.AddListener(HandleSkip);
            _BtnEsc.onClick.AddListener(HandleEsc);
            
            
            executor.OnNodeAdded += OnNodeAdded;

            // Dialog playback is started explicitly through Play so callers can choose the tree.
        }

        public void Play(DialogTree tree, Action completed = null)
        {
            onCompleted = completed;
            completionNotified = false;
            StartDialogTree(tree);
        }

        private void StartDialogTree(DialogTree tree)
        {
            if (tree == null)
            {
                Debug.LogError("DialogUIView StartDialogTree failed: dialog tree is null.");
                NotifyCompleted();
                return;
            }

            executor.StartDialog(tree);
            
        }
        #region 事件处理
        private void HandleEsc()
        {
            if (debug)
            {
                Debug.Log("HandleEsc");
            }
            NotifyCompleted();
            GameViewManager.UIManager.Close(this);
        }

        private void HandleSkip()
        {
            if (debug)
            {
                Debug.Log("HandleSkip");
            }
            //TODO：这里应该触发所有的奖励
        }

        private void HandleNext()
        {
            if (debug)
            {
                Debug.Log("HandleNext");
            }
            bool isEnd = false;
            executor.ExecuteNode(out isEnd);
            if (isEnd)
            {
                HandleEsc();
            }
        }

        private void HandleAuto()
        {
            
            if (debug)
            {
                Debug.Log("HandleAuto");
            }
        }

        private void HandleShowReview()
        {
            if (debug)
            {
                Debug.Log("HandleShowReview");
            }
        }
        #endregion
        private void OnNodeAdded(string guid, BaseDialogNode node)
        {
            if (node is DialogRootNode rootNode)
            {
                return;
            }
            if (node is SimpleDialogNode simpleDialogNode)
            {
                dialogAdapter.Add(new DialogText
                {
                    Speaker = simpleDialogNode.speaker,
                    Content = simpleDialogNode.content
                });
                
                speakerText.text = simpleDialogNode.speaker;
                contentText.text = simpleDialogNode.content;
            }

            if (node is OptionDialogNode optionDialogNode)
            {
                optionPool.SetActive(true);
                _BtnOption1.onClick.RemoveAllListeners();
                _BtnOption2.onClick.RemoveAllListeners();
                _BtnOption3.onClick.RemoveAllListeners();
                switch (optionDialogNode.options.Count)
                {
                    case 1:
                        _BtnOption1.gameObject.SetActive(true);
                        _BtnOption2.gameObject.SetActive(false);
                        _BtnOption3.gameObject.SetActive(false);
                        //设置选项文本
                        _option1Text.text = optionDialogNode.options[0].text;
                        _BtnOption1.onClick.AddListener(() => HandleSelectedOption(optionDialogNode,0));
                        break;
                    case 2:
                        _BtnOption1.gameObject.SetActive(true);
                        _BtnOption2.gameObject.SetActive(true);
                        _BtnOption3.gameObject.SetActive(false);
                        //设置选项文本
                        _option1Text.text = optionDialogNode.options[0].text; 
                        _option2Text.text = optionDialogNode.options[1].text;
                        _BtnOption1.onClick.AddListener(() => HandleSelectedOption(optionDialogNode,0));
                        _BtnOption2.onClick.AddListener(() => HandleSelectedOption(optionDialogNode,1));
                        break;
                    case 3:
                        _BtnOption1.gameObject.SetActive(true);
                        _BtnOption2.gameObject.SetActive(true);
                        _BtnOption3.gameObject.SetActive(true);
                        //设置选项文本
                        _option1Text.text = optionDialogNode.options[0].text;
                        _option2Text.text = optionDialogNode.options[1].text;
                        _option3Text.text = optionDialogNode.options[2].text;
                        _BtnOption1.onClick.AddListener(() => HandleSelectedOption(optionDialogNode,0));
                        _BtnOption2.onClick.AddListener(() => HandleSelectedOption(optionDialogNode,1));
                        _BtnOption3.onClick.AddListener(() => HandleSelectedOption(optionDialogNode,2));
                        break;
                    default:
                        throw new System.Exception("OptionDialogNode must have 1-3 children");
                }
            }
        }
        private void HandleSelectedOption(OptionDialogNode optionDialogNode,int index)
        {
            optionDialogNode.SetOptionIndex(index);
            optionPool.SetActive(false);
            HandleNext();
        }

        private void NotifyCompleted()
        {
            if (completionNotified)
            {
                return;
            }

            completionNotified = true;
            onCompleted?.Invoke();
            onCompleted = null;
        }
    }
}
