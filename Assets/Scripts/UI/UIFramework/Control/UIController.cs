using System.Collections.Generic;
using Common;
using Common.Constant;
using UI.UIFramework.Data;
using UI.UIFramework.Interact;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using PlayerInputManager = Tools.InputHandle.PlayerInputManager;

namespace UI.UIFramework.Control
{
    /// <summary>
    /// 不需要跨场景，每个场景存在一个UIController，用于对UI元素进行管理
    /// </summary>
    public class UIController:MonoBehaviour
    {
        public static UIController instance;
        [SerializeField] GamePanel firstSelectedPanel;
        
        //for cur UI info
        public PanelName CurPanel { get; private set; }
        public SubPanelName CurSubPanel { get; private set; }
        public OptionName CurOption { get; private set; }
        public int CurButtonID { get; private set; }

        //for getting GamePanel
        GamePanel[] allGamePanels;
        Dictionary<PanelName, GamePanel> dicPanels;
        List<PanelName> activePanels;

        //for getting GameSubPanel
        GameSubPanel[] allSubPanels;
        Dictionary<SubPanelName, GameSubPanel> dicSubPanels;

        //for getting BtnOption and GameButton
        ButtonOption[] options;
        Dictionary<OptionName, ButtonOption> dicOptions;
        GameButton[] gameButtons;
        Dictionary<int, GameButton> dicButtons;
        
        [Header("-for monitoring-")]
        [SerializeField] GameStatus _curStatus;
        [SerializeField] string _curScene, _curPanel, _curSubPanel, _curOption, _curInputType;
        int buttonAmount, textAmount;
        
        void Awake()
        {
            if (!instance)
                instance = this;
            else
                Destroy(gameObject);

            //获得当前场景中的所有GamePanel, GameSubPanel, BtnOption, GameButton
            InitButtonAndOption(); //must be first
            InitPanelsAndSubPanels();
        }

        private void OnEnable()
        {
            GameApplication.Instance.EventCenter.AddEventListener
                (GlobalEvents.OnRebindStarted, OnRebindingStarted);
            GameApplication.Instance.EventCenter.AddEventListener
                (GlobalEvents.OnRebindCompleted, OnRebindingCompleted);
            
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.OpenMenu,HandleOpenMenu);
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.Cancel, HandleCancel);
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.Click, HandleClick);
        }
        
        private void OnDisable()
        {
            GameApplication.Instance?.EventCenter.RemoveEventListener
                (GlobalEvents.OnRebindStarted, OnRebindingStarted);
            GameApplication.Instance?.EventCenter.RemoveEventListener
                (GlobalEvents.OnRebindCompleted, OnRebindingCompleted);
            
            if (PlayerInputManager.HasInstance)
            {
                PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.OpenMenu,HandleOpenMenu);
                PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.Cancel, HandleCancel);
                PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.Click, HandleClick);
            }
        }
        private void HandleCancel(InputAction.CallbackContext obj)
        {
            if (CurSubPanel != SubPanelName.None) //优先关闭CurSubPanel
                CloseSubPanel();
            else
            {
                switch (CurPanel) //然后关闭CurPanel
                {
                    case PanelName.PanelPause: //关闭PanelPause时，重置游戏状态
                        ContinueGame();
                        break;
                    case PanelName.PanelMenu: //PanelMenu不可被关闭, 因此break
                        break;
                    default:
                        CloseCurPanel();
                        break;
                }
            }
        }

        private void HandleClick(InputAction.CallbackContext obj)
        {
            if (!IsCurOption(OptionName.None))
            {
                EventSystem.current.SetSelectedGameObject(dicOptions[CurOption].gameObject);
            }
            else if (CurButtonID != 0)
            {
                EventSystem.current.SetSelectedGameObject(dicButtons[CurButtonID].gameObject);
            }
        }

        private void HandleOpenMenu(InputAction.CallbackContext obj)
        {
           PauseGame();
        }
        
        public void PauseGame()
        {
            if (GameManager.CurGameStatus != GameStatus.Paused && CurPanel != PanelName.PanelPause)
            {
                OpenPanel(PanelName.PanelPause);
                GameManager.SetGameStatus(GameStatus.Paused);
            }
        }

        public void ContinueGame()
        {
            CloseCurPanel();
            GameManager.SetGameStatus(GameStatus.Playing);
        }
        public void OpenPanel(PanelName m_panelName)
        {
            AddActivePanel(m_panelName);
            dicPanels[m_panelName].ShowPanel();
        }
        void AddActivePanel(PanelName _panelName)
        {
            CurPanel = _panelName;
            if (activePanels.Count > 0)
            {
                PanelName _cur = activePanels[activePanels.Count - 1];
                dicPanels[_cur].HidePanel();
            }
            activePanels.Add(_panelName);
        }
        void RemoveActivePanel(PanelName _panelName)
        {
            activePanels.Remove(_panelName);
            if (activePanels.Count > 0)
            {
                PanelName _cur = activePanels[activePanels.Count - 1];
                dicPanels[_cur].ShowPanel();
                CurPanel = _cur;
            }
            else
                CurPanel = PanelName.None;
        }
        #region GameSubPanel
        public void OpenSubPanel(SubPanelName _panelName)
        {
            dicPanels[CurPanel].DisablePanelButtons();
            dicSubPanels[_panelName].ShowSubPanel();
            SetCurSubPanel(_panelName);
        }

        public void CloseSubPanel()
        {
            UIDataManager.Instance.SaveCurrentSettings();
            GameApplication.Instance.EventCenter.TriggerEvent(GlobalEvents.OnSettingsSaved);
            dicSubPanels[CurSubPanel].HideSubPanel();
            dicPanels[CurPanel].EnablePanelButtons();
            ClearCurSubPanel();
            ClearCurOption();
        }

        void SetCurSubPanel(SubPanelName _panelName)
        {
            CurSubPanel = _panelName;
        }

        public void ClearCurSubPanel()
        {
            if (CurSubPanel == SubPanelName.None)
                return;
            CurSubPanel = SubPanelName.None;
        }
        #endregion

        public void CloseCurPanel()
        {
            if (CurSubPanel != SubPanelName.None) //如果有SubPanel打开，则Panel不能被关闭（优先关闭SubPanel）
                return;

            dicPanels[CurPanel].HidePanel(); //关闭当前Panel
            RemoveActivePanel(CurPanel);
            ClearCurOption();
        }
        public void SetCurOption(OptionName _optName)
        {
            CurOption = _optName;
            ClearCurBtnID();
        }
        public void ClearCurOption()
        {
            CurOption = OptionName.None;
        }

        public bool IsCurOption(OptionName _optName)
        {
            return CurOption == _optName;
        }

        public void SetLastFocusBtn(int m_id)
        {
            if (dicButtons.ContainsKey(m_id))
                EventSystem.current.SetSelectedGameObject(dicButtons[m_id].gameObject);
        }
        void InitPanelsAndSubPanels()
        {
            dicPanels = new Dictionary<PanelName, GamePanel>();
            activePanels = new List<PanelName>();
            dicSubPanels = new Dictionary<SubPanelName, GameSubPanel>();

            allGamePanels = FindObjectsOfType<GamePanel>(true);
            allSubPanels = FindObjectsOfType<GameSubPanel>(true);
            foreach (GamePanel panel in allGamePanels)
            {
                dicPanels.Add(panel.panelName, panel);
                panel.InitPanel();
            }
            foreach (GameSubPanel subPanel in allSubPanels)
            {
                dicSubPanels.Add(subPanel.subpanelName, subPanel);
                subPanel.InitSubPanel();
            }
            if (firstSelectedPanel)
                OpenPanel(firstSelectedPanel.panelName);
        }
        void InitButtonAndOption()
        {
            dicOptions = new Dictionary<OptionName, ButtonOption>();
            options = FindObjectsOfType<ButtonOption>(true);

            if (options.Length != 0)
            {
                foreach (ButtonOption option in options)
                {
                    dicOptions.Add(option.optionName, option);
                }
            }

            dicButtons = new Dictionary<int, GameButton>();
            gameButtons = FindObjectsOfType<GameButton>(true);
            if (gameButtons.Length != 0)
            {
                for (int i = 0; i < gameButtons.Length; i++)
                {
                    int m_id = i + 1;
                    dicButtons.Add(m_id, gameButtons[i]);
                    gameButtons[i].SetButtonID(m_id);
                }
            }
        }

        public void SetCurBtnID(int id)
        {
            CurButtonID = id;
            ClearCurOption();
        }

        public void ClearCurBtnID()
        {
            CurButtonID = 0;
        }
        
        #region Event
        public void OnRebindingStarted()
        {
            if (CurSubPanel == SubPanelName.Keyboard || CurSubPanel == SubPanelName.Gamepad)
            {
                dicSubPanels[CurSubPanel].DisableSubPanelButtons();
            }
        }

        public void OnRebindingCompleted()
        {
            if (CurSubPanel == SubPanelName.Keyboard || CurSubPanel == SubPanelName.Gamepad)
            {
                dicSubPanels[CurSubPanel].EnableSubPanelButtons();
            }
        }
        #endregion
    }
}
