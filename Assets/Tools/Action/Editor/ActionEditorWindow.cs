using System;
using System.Collections.Generic;
using Animancer;
using Tools.Action.Data;
using Tools.Action.Editor.Channel;
using Tools.Action.Editor.Track;
using Tools.Action.Running;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Tools.Action.Editor
{
    public enum ActionTrackType
    {
        Animation,
        Audio,
        Particle,
        Collision,
        Event
    }
    public class ActionEditorWindow : EditorWindow
    {
        public static ActionEditorWindow Instance;

        // 轨道事件
        public event System.Action OnTracksRefreshed;
        
         #region 预设值

        // 编辑器场景
        private const string EditorScenePath = "Assets/Tools/Action/Editor/Test/ActionTest.unity";

        // 帧时间刻度像素步长
        public const float StandardFrameTimescaleUnitStepPixel = 6f;

        // 帧时间刻度缩放系数
        private const float MinFrameTimescaleScaleFactor = 1f;
        private const float MaxFrameTimescaleScaleFactor = 15f;

        // 帧时间刻度标展示优先级
        private const int FrameTimescalePriority1Step = 10;
        private const float FrameTimescalePriority1LinePercentage = 0.4f;
        private const int FrameTimescalePriority2Step = 5;
        private const float FrameTimescalePriority2LinePercentage = 0.3f;
        private const int FrameTimescalePriority3Step = 1;
        private const float FrameTimescalePriority3LinePercentage = 0.2f;

        // 帧时间刻度数字标识步长
        private const float StandardFrameTimescaleTickStep = 10;

        // 帧时间选择线拖动像素范围
        private const float FrameSelectedLineDragPixelRange = StandardFrameTimescaleUnitStepPixel;

        // 通道和轨道颜色
        public static readonly Color PrimaryChannelColor = new Color(88 / 256f, 88 / 256f, 88 / 256f);
        public static readonly Color SecondaryChannelColor = new Color(42 / 256f, 42 / 256f, 42 / 256f);
        public static readonly Color ProcessTrackNormalColor = new Color(241 / 256f, 196 / 256f, 15 / 256f);
        public static readonly Color ProcessTrackSelectedColor = new Color(247 / 256f, 220 / 256f, 111 / 256f);
        public static readonly Color AnimationTrackNormalColor = new Color(52 / 256f, 152 / 256f, 219 / 256f);
        public static readonly Color AnimationTrackSelectedColor = new Color(133 / 256f, 193 / 256f, 233 / 256f);
        public static readonly Color AudioTrackNormalColor = new Color(46 / 256f, 204 / 256f, 113 / 256f);
        public static readonly Color AudioTrackSelectedColor = new Color(130 / 256f, 224 / 256f, 170 / 256f);
        public static readonly Color EffectTrackNormalColor = new Color(116 / 256f, 0 / 256f, 184 / 256f);
        public static readonly Color EffectTrackSelectedColor = new Color(200 / 256f, 182 / 256f, 255 / 256f);
        public static readonly Color CollideDetectionTrackNormalColor = new Color(239 / 256f, 35 / 256f, 60 / 256f);
        public static readonly Color CollideDetectionTrackSelectedColor = new Color(255 / 256f, 112 / 256f, 166 / 256f);
        public static readonly Color EventTrackNormalColor = new Color(241 / 256f, 196 / 256f, 15 / 256f);
        public static readonly Color EventTrackSelectedColor = new Color(247 / 256f, 220 / 256f, 111 / 256f);

        #endregion

        #region 头部菜单控件

        private Button _btnGotoEditorScene;
        private Button _btnReturnLastScene;
        private ObjectField _ofModelPrefab;
        private Button _btnLoadModel;
        private ObjectField _ofFileConfig;
        private TextField _tfActionName;
        private Button _btnSaveConfig;

        private string _lastScenePath;
        private GameObject _modelPrefab;
        private GameObject _modelPrefabInstance;
        public GameObject PreviewInstance => _modelPrefabInstance;

        private ActionClip _actionClip;
        private bool _configInitial;

        #endregion

        //播放操作暂未添加
        #region 帧时间轴控件

        private Button _btnPreviousFrame;
        private Button _btnPlayOrPause;
        private Button _btnNextFrame;
        private TextField _tfCurrentFrame;
        private TextField _tfTotalFrames;
        private EnumField _efFrameRate;
        private IMGUIContainer _barFrameTimescale;
        private VisualElement _lineFrameSelected;
        private VisualElement _lineFrameSelectedColor;

        private bool _playing;
        private int _startFrameWhenPlaying;
        private DateTime _startTimeWhenPlaying;
        private DateTime _lastPlayingTime;

        private IActionClipPlay _actionClipPlay;

        private IActionClipPlay ActionClipPlay
        {
            get
            {
                // 在初始化动作文件时不可使用动作播放器
                if (_configInitial)
                {
                    return null;
                }

                if (_actionClipPlay == null)
                {
                    if (_modelPrefabInstance && _actionClip)
                    {
                        /*_actionClipPlay = new ActionClipEditorPlayer(
                            _modelPrefabInstance.GetComponent<AnimancerComponent>(),
                            _modelPrefabInstance.transform);*/
                    }
                }

                if (_actionClipPlay != null && !Playing && _actionClip)
                {
                    var actionClip = Instantiate(_actionClip);
                    /*SetNewestActionData(actionClip);
                    _actionClipPlay.ChangeActionClip(actionClip);*/
                }

                return _actionClipPlay;
            }
        }

        public bool Playing
        {
            get => _playing;
            set
            {
                if (value)
                {
                    _btnPlayOrPause.text = "\u25a0";
                    _btnPlayOrPause.style.backgroundColor = new Color(256 / 256f, 110 / 256f, 0 / 256f);
                    // 如果当前帧是最后一帧就跳转到0帧
                    if (SelectedFrame == TotalFrames)
                    {
                        SelectedFrame = 0;
                    }

                    // 记录播放开始帧
                    _startTimeWhenPlaying = DateTime.Now;
                    _lastPlayingTime = DateTime.Now;
                    _startFrameWhenPlaying = SelectedFrame;
                    /*ActionClipPlay?.StartAt(_startFrameWhenPlaying);*/
                }
                else
                {
                    _btnPlayOrPause.text = "\u25b6";
                    _btnPlayOrPause.style.backgroundColor = new Color(88 / 256f, 88 / 256f, 88 / 256f);
                    /*ActionClipPlay?.Stop();*/
                }

                _playing = value;
            }
        }

        public int FrameRate { get; set; }

        public float FrameTimeUnit => 1f / FrameRate;

        private float _frameTimescaleScaleFactor;
        public float FrameTimescaleScaleFactor => _frameTimescaleScaleFactor;

        private int _selectedFrame;

        public int SelectedFrame
        {
            get => _selectedFrame;
            private set
            {
                if (_selectedFrame == value)
                {
                    return;
                }

                _selectedFrame = value;
                if (_selectedFrame > TotalFrames)
                {
                    TotalFrames = _selectedFrame;
                }

                _tfCurrentFrame.value = _selectedFrame.ToString();
                // 时间刻度重绘
                _barFrameTimescale.MarkDirtyLayout();
                if (!Playing)
                {
                    // 播放动作内容
                    /*ActionClipPlay?.PlayAt(value);*/
                }
            }
        }

        private int _totalFrames;

        public int TotalFrames
        {
            get => _totalFrames;
            private set
            {
                if (_totalFrames == value)
                {
                    return;
                }

                _totalFrames = value;
                if (_totalFrames < SelectedFrame)
                {
                    SelectedFrame = _totalFrames;
                }
                else
                {
                    // 执行轨道刷新事件
                    OnTracksRefreshed?.Invoke();
                }

                _tfTotalFrames.value = _totalFrames.ToString();
                // 重新设置轨道列表宽度
                _listTrackList.style.width =
                    StandardFrameTimescaleUnitStepPixel * _frameTimescaleScaleFactor * _totalFrames;
            }
        }

        private bool _dragSelectedLine;
        public bool DragSelectedLine => _dragSelectedLine;

        public float FrameTimescalePixelOffset => Mathf.Abs(_elementTrackListContainer.transform.position.x);

        #endregion
        
        #region 内容控件
        private ScrollView _svChannelList;
        private ListView _listChannelList;
        private VisualTreeAsset _channelListItem_uxml;
        private StyleSheet _channelListItem_styleSheet; 

        private ScrollView _svTrackList;
        private ListView _listTrackList;
        private VisualElement _elementTrackListContainer;
        #endregion
        
        private DateTime _timerScheduleTime;
        private float _timerTriggerTime;
        private UnityAction _onTimerTriggered;
        
        //channel列表
        ///给listview用
        private List<ChannelBase> _channelViewEditors = new List<ChannelBase>();
        
        /// 不同类型channel存储的子通道
        private List<ChannelBase> _animationChannels = new List<ChannelBase>();
        private List<ChannelBase> _audioChannels = new List<ChannelBase>();
        private List<ChannelBase> _particleChannels = new List<ChannelBase>();
        private List<ChannelBase> _collisionChannels = new List<ChannelBase>();   
        private List<ChannelBase> _eventChannels = new List<ChannelBase>();
        
        //track列表
        ///给listview用
        private List<ActionTrackUI> _trackUIs = new List<ActionTrackUI>();
        

        [MenuItem("Window/UI Toolkit/ActionEditorWindow")]
        public static void ShowExample()
        {
            ActionEditorWindow wnd = GetWindow<ActionEditorWindow>();
            wnd.titleContent = new GUIContent("ActionEditorWindow");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if(Selection.activeObject is ActionClip actionClip)
            {
                ShowExample();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            //import uxml 
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Tools/Action/Editor/ActionEditorWindow.uxml");
            visualTree.CloneTree(root);

            //add uss
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Tools/Action/Editor/ActionEditorWindow.uss");
            root.styleSheets.Add(styleSheet);
            
            //channel列表项模板
            
            _channelListItem_uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Tools/Action/Editor/ChannelListItem.uxml");
            _channelListItem_styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Tools/Action/Editor/ChannelListItem.uss");
                
            #region 绑定控件

            _btnGotoEditorScene = rootVisualElement.Q<Button>("BtnGotoEditorScene");
            _btnReturnLastScene = rootVisualElement.Q<Button>("BtnReturnLastScene");
            _ofModelPrefab = rootVisualElement.Q<ObjectField>("OfModelPrefab");
            _btnLoadModel = rootVisualElement.Q<Button>("BtnLoadModel");
            _ofFileConfig = rootVisualElement.Q<ObjectField>("OfFileConfig");
            _tfActionName = rootVisualElement.Q<TextField>("TfActionName");
            _btnSaveConfig = rootVisualElement.Q<Button>("BtnSaveConfig");

            _btnPreviousFrame = rootVisualElement.Q<Button>("BtnPreviousFrame");
            _btnPlayOrPause = rootVisualElement.Q<Button>("BtnPlayOrPause");
            _btnNextFrame = rootVisualElement.Q<Button>("BtnNextFrame");
            _tfCurrentFrame = rootVisualElement.Q<TextField>("TfCurrentFrame");
            _tfTotalFrames = rootVisualElement.Q<TextField>("TfTotalFrames");
            _efFrameRate = rootVisualElement.Q<EnumField>("EfFrameRate");
            _barFrameTimescale = rootVisualElement.Q<IMGUIContainer>("BarFrameTimescale");
            _lineFrameSelected = rootVisualElement.Q<VisualElement>("LineFrameSelected");
            _lineFrameSelectedColor = rootVisualElement.Q<VisualElement>("LineFrameSelectedColor");

            _svChannelList = rootVisualElement.Q<ScrollView>("SvChannelList");
            _listChannelList = _svChannelList.Q<ListView>("LvChannelList");
            _svTrackList = rootVisualElement.Q<ScrollView>("SvTrackList");
            _elementTrackListContainer = _svTrackList.Q<VisualElement>("unity-content-container");
            _listTrackList = _svTrackList.Q<ListView>("LvTrackList");

            #endregion
            
            //初始化所有控件
            InitFrameBar();
            InitTopMenu();
            InitChannelConfig();

        }
        #region 时间轴相关
        private void InitFrameBar()
        {
            // 初始化数据
            Playing = false;
            _frameTimescaleScaleFactor = MinFrameTimescaleScaleFactor;
            SelectedFrame = 0;
            TotalFrames = 100;
            FrameRate = (int)(ActionEditorFrameRate)_efFrameRate.value;
            _dragSelectedLine = false;

            _btnPreviousFrame.clicked += ClickPreviousFrameButton;
            _btnPlayOrPause.clicked += ClickPlayOrPauseButton;
            _btnNextFrame.clicked += ClickNextFrameButton;
            _tfCurrentFrame.RegisterValueChangedCallback(HandleCurrentFrameTextFieldValueChanged);
            _tfTotalFrames.RegisterValueChangedCallback(HandleTotalFramesTextFieldValueChanged);
            _efFrameRate.RegisterValueChangedCallback(HandleFrameRateEnumFieldValueChanged);

            _barFrameTimescale.onGUIHandler = DrawTimescaleBar;
            _barFrameTimescale.RegisterCallback<WheelEvent>(HandleTimescaleWheelEvent);
            _barFrameTimescale.RegisterCallback<MouseDownEvent>(HandleTimescaleMouseDownEvent);
            _barFrameTimescale.RegisterCallback<MouseMoveEvent>(HandleTimescaleMouseMoveEvent);
            _barFrameTimescale.RegisterCallback<MouseUpEvent>(HandleTimescaleMouseUpEvent);
        }

        private void DestroyFrameBar()
        {
            _btnPreviousFrame.clicked -= ClickPreviousFrameButton;
            _btnPlayOrPause.clicked -= ClickPlayOrPauseButton;
            _btnNextFrame.clicked -= ClickNextFrameButton;
            _tfCurrentFrame.UnregisterValueChangedCallback(HandleCurrentFrameTextFieldValueChanged);
            _tfTotalFrames.UnregisterValueChangedCallback(HandleTotalFramesTextFieldValueChanged);
            _efFrameRate.UnregisterValueChangedCallback(HandleFrameRateEnumFieldValueChanged);

            _barFrameTimescale.onGUIHandler = null;
            _barFrameTimescale.UnregisterCallback<WheelEvent>(HandleTimescaleWheelEvent);
            _barFrameTimescale.UnregisterCallback<MouseDownEvent>(HandleTimescaleMouseDownEvent);
            _barFrameTimescale.UnregisterCallback<MouseMoveEvent>(HandleTimescaleMouseMoveEvent);
            _barFrameTimescale.UnregisterCallback<MouseUpEvent>(HandleTimescaleMouseUpEvent);
            _barFrameTimescale.UnregisterCallback<MouseOutEvent>(HandleTimescaleMouseOutEvent);
        }
        private void HandleTimescaleWheelEvent(WheelEvent wheelEvent)
        {
            // 调整当前时间帧缩放系数，delta.y为正数代表滚轮向下即放大，delta.y为负数代表滚轮向上即缩小
            _frameTimescaleScaleFactor = Mathf.Clamp(_frameTimescaleScaleFactor - wheelEvent.delta.y / 5,
                MinFrameTimescaleScaleFactor, MaxFrameTimescaleScaleFactor);
            // 时间刻度重绘
            _barFrameTimescale.MarkDirtyLayout();
            // 重新设置轨道列表宽度
            _listTrackList.style.width = StandardFrameTimescaleUnitStepPixel * _frameTimescaleScaleFactor * _totalFrames;
        }

        private void HandleTimescaleMouseDownEvent(MouseDownEvent mouseDownEvent)
        {
            // 如果鼠标落在拖动范围外，则重新设置选择线对应的帧数
            if (Mathf.Abs(_lineFrameSelected.transform.position.x -
                          (mouseDownEvent.mousePosition.x - _barFrameTimescale.worldTransform.GetPosition().x)) >
                FrameSelectedLineDragPixelRange)
            {
                // 设置选中帧数
                SelectedFrame = CalculateTimescalePositionNearFrame(
                    mouseDownEvent.mousePosition.x - _barFrameTimescale.worldTransform.GetPosition().x
                );
            }

            _dragSelectedLine = true;
        }

        private void HandleTimescaleMouseMoveEvent(MouseMoveEvent mouseMoveEvent)
        {
            if (!_dragSelectedLine)
            {
                return;
            }

            // 设置选中帧数
            SelectedFrame = CalculateTimescalePositionNearFrame(
                mouseMoveEvent.mousePosition.x - _barFrameTimescale.worldTransform.GetPosition().x
            );
        }

        private void HandleTimescaleMouseUpEvent(MouseUpEvent mouseUpEvent)
        {
            if (!_dragSelectedLine)
            {
                return;
            }

            // 设置选中帧数
            SelectedFrame = CalculateTimescalePositionNearFrame(
                mouseUpEvent.mousePosition.x - _barFrameTimescale.worldTransform.GetPosition().x
            );

            _dragSelectedLine = false;
        }

        private void HandleTimescaleMouseOutEvent(MouseOutEvent mouseOutEvent)
        {
            if (!_dragSelectedLine)
            {
                return;
            }

            // 设置选中帧数
            SelectedFrame = CalculateTimescalePositionNearFrame(
                mouseOutEvent.mousePosition.x - _barFrameTimescale.worldTransform.GetPosition().x
            );

            _dragSelectedLine = false;
        }

        public int CalculateTimescalePositionNearFrame(float timescalePosition, bool withPixelOffset = true)
        {
            var timescalePixelOffset = (withPixelOffset ? FrameTimescalePixelOffset : 0f) + timescalePosition;
            var frameTimescaleUnitStepPixel = StandardFrameTimescaleUnitStepPixel * _frameTimescaleScaleFactor;
            var passedFrames =
                Mathf.FloorToInt(timescalePixelOffset / frameTimescaleUnitStepPixel);
            var firstFramePixelDistance =
                (passedFrames + 1) * frameTimescaleUnitStepPixel - timescalePixelOffset;

            if (firstFramePixelDistance <= frameTimescaleUnitStepPixel / 2)
            {
                return passedFrames + 1;
            }

            return passedFrames;
        }
        private void DrawTimescaleBar()
        {
            Handles.BeginGUI();
            var timescaleContentRect = _barFrameTimescale.contentRect;
            var frameTimescaleUnitStepPixel = StandardFrameTimescaleUnitStepPixel * _frameTimescaleScaleFactor;

            // 计算当前偏移后的第一帧是多少帧，从控件开始距离第一帧的距离是多少
            var passedFrames = Mathf.FloorToInt(FrameTimescalePixelOffset / frameTimescaleUnitStepPixel);
            var firstFramePixelDistance =
                (passedFrames + 1) * frameTimescaleUnitStepPixel - FrameTimescalePixelOffset;

            // 这里额外处理，为了让刚好出现的第一帧能够显示出来
            int frame;
            if (Mathf.Approximately(firstFramePixelDistance, frameTimescaleUnitStepPixel))
            {
                frame = passedFrames;
                firstFramePixelDistance = 0f;
            }
            else
            {
                frame = passedFrames + 1;
            }

            // 计算帧记号步长
            var frameTimescaleTickStep = MathF.Ceiling(StandardFrameTimescaleTickStep / _frameTimescaleScaleFactor);

            bool findSelectedFrame = false;

            for (float timescalePixel = firstFramePixelDistance;
                 timescalePixel < timescaleContentRect.width;
                 timescalePixel += frameTimescaleUnitStepPixel)
            {
                // 展示记号帧数
                if (frame % frameTimescaleTickStep == 0)
                {
                    Handles.color = Color.white;
                    var frameStr = frame.ToString();
                    GUI.Label(new Rect(timescalePixel - frameStr.Length * 4.5f, timescaleContentRect.y - 3, 35, 20),
                        frameStr);
                }

                // 展示帧刻度线
                if (frame % FrameTimescalePriority1Step == 0)
                {
                    Handles.color = Color.white;
                    Handles.DrawLine(
                        new Vector2(timescalePixel,
                            Mathf.Clamp01(1 - FrameTimescalePriority1LinePercentage) * timescaleContentRect.height),
                        new Vector2(timescalePixel, timescaleContentRect.height)
                    );
                }

                else if (frame % FrameTimescalePriority2Step == 0)
                {
                    Handles.color = Color.white;
                    Handles.DrawLine(
                        new Vector2(timescalePixel,
                            Mathf.Clamp01(1 - FrameTimescalePriority2LinePercentage) * timescaleContentRect.height),
                        new Vector2(timescalePixel, timescaleContentRect.height)
                    );
                }
                else if (frame % FrameTimescalePriority3Step == 0)
                {
                    Handles.color = Color.grey;
                    Handles.DrawLine(
                        new Vector2(timescalePixel,
                            Mathf.Clamp01(1 - FrameTimescalePriority3LinePercentage) * timescaleContentRect.height),
                        new Vector2(timescalePixel, timescaleContentRect.height)
                    );
                }

                // 如果找到选中帧，就展示选中线以及选中帧数
                if (frame == SelectedFrame)
                {
                    findSelectedFrame = true;

                    if (_dragSelectedLine)
                    {
                        _lineFrameSelectedColor.style.backgroundColor = Color.grey;
                    }
                    else
                    {
                        _lineFrameSelectedColor.style.backgroundColor = Color.white;
                    }

                    var originPosition = _lineFrameSelected.transform.position;
                    _lineFrameSelected.visible = true;
                    _lineFrameSelected.transform.position =
                        new Vector3(timescalePixel, originPosition.y, originPosition.z);
                }

                frame++;
            }

            // 如果没找到选中帧，就隐藏选中线
            if (!findSelectedFrame)
            {
                _lineFrameSelected.visible = false;
            }

            Handles.EndGUI();

            // 执行轨道刷新事件
            OnTracksRefreshed?.Invoke();
        }
        private void ClickPreviousFrameButton()
        {
            SelectedFrame = Mathf.Max(SelectedFrame - 1, 0);
        }

        private void ClickPlayOrPauseButton()
        {
            Playing = !Playing;
        }

        private void ClickNextFrameButton()
        {
            SelectedFrame++;
        }

        private void HandleCurrentFrameTextFieldValueChanged(ChangeEvent<string> value)
        {
            int newValue;
            if (String.IsNullOrEmpty(value.newValue))
            {
                newValue = 0;
            }
            else
            {
                newValue = Mathf.Max(int.Parse(value.newValue), 0);
            }

            SelectedFrame = newValue;
        }
        private void HandleTotalFramesTextFieldValueChanged(ChangeEvent<string> value)
        {
            int newValue;
            if (String.IsNullOrEmpty(value.newValue))
            {
                newValue = 0;
            }
            else
            {
                newValue = Mathf.Max(int.Parse(value.newValue), 0);
            }

            TotalFrames = newValue;
        }
        private void HandleFrameRateEnumFieldValueChanged(ChangeEvent<Enum> value)
        {
            var oldFrameRate = FrameRate;
            FrameRate = (int)(ActionEditorFrameRate)value.newValue;
            // 设置总帧数
            TotalFrames = _actionClip != null
                ? Mathf.RoundToInt(_actionClip.duration * FrameRate)
                : Mathf.RoundToInt(TotalFrames * 1f * FrameRate / oldFrameRate);
            /*// 同步各通道下轨道的时间
            SynchronizeChannelTime(_processChannelGroup.ChildChannels);
            SynchronizeChannelTime(_animationChannelGroup.ChildChannels);
            SynchronizeChannelTime(_audioChannelGroup.ChildChannels);
            SynchronizeChannelTime(_effectChannelGroup.ChildChannels);
            SynchronizeChannelTime(_collideDetectionChannelGroup.ChildChannels);
            SynchronizeChannelTime(_eventChannelGroup.ChildChannels);

            return;

            void SynchronizeChannelTime(List<IActionChannel> channels)
            {
                foreach (var channel in channels)
                {
                    foreach (var track in channel.Tracks)
                    {
                        track.Data.UpdateFrameRate(FrameRate);
                        track.Bind(track.Data);
                    }
                }
            }*/
        }


        #endregion
        #region 顶部控件
        private void InitTopMenu()
        {
            _btnGotoEditorScene.clicked += ClickGotoEditorSceneButton;
            _btnReturnLastScene.clicked += ClickReturnLastSceneButton;
            _btnLoadModel.clicked += ClickLoadModelButton;
            _btnSaveConfig.clicked += ClickSaveConfigButton;
            _ofModelPrefab.RegisterValueChangedCallback(HandleModelPrefabObjectFieldValueChanged);
            _ofFileConfig.RegisterValueChangedCallback(HandleConfigFileObjectFieldValueChanged);

            _ofModelPrefab.value = _modelPrefab;
        }

        private void DestroyTopMenu()
        {
            _btnGotoEditorScene.clicked -= ClickGotoEditorSceneButton;
            _btnReturnLastScene.clicked -= ClickReturnLastSceneButton;
            _btnLoadModel.clicked -= ClickLoadModelButton;
            _btnSaveConfig.clicked -= ClickSaveConfigButton;
            _ofModelPrefab.UnregisterValueChangedCallback(HandleModelPrefabObjectFieldValueChanged);
            _ofFileConfig.UnregisterValueChangedCallback(HandleConfigFileObjectFieldValueChanged);
        }
        private void ClickGotoEditorSceneButton()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == EditorScenePath)
            {
                return;
            }

            if (_modelPrefabInstance)
            {
                DestroyImmediate(_modelPrefabInstance);
            }

            ScheduleTimer(0.1f, () =>
            {
                _lastScenePath = activeScene.path;
                EditorSceneManager.OpenScene(EditorScenePath);
            });
        }

        private void ClickReturnLastSceneButton()
        {
            if (String.IsNullOrEmpty(_lastScenePath))
            {
                return;
            }

            if (_modelPrefabInstance)
            {
                DestroyImmediate(_modelPrefabInstance);
            }


            ScheduleTimer(0.1f, () =>
            {
                EditorSceneManager.OpenScene(_lastScenePath);
                _lastScenePath = "";
            });
        }
        private void ClickLoadModelButton()
        {
            if (!_modelPrefab)
            {
                return;
            }

            if (SceneManager.GetActiveScene().path != EditorScenePath)
            {
                return;
            }

            if (_modelPrefabInstance)
            {
                DestroyImmediate(_modelPrefabInstance);
            }

            _modelPrefabInstance = GameObject.Instantiate(_modelPrefab, Vector3.zero, Quaternion.identity);
            InitModelInstance(_modelPrefabInstance);

            // 重新初始化播放器
            if (_modelPrefabInstance && _actionClip)
            {
                /*_actionClipPlay = new ActionClipEditorPlayer(_modelPrefabInstance.GetComponent<AnimancerComponent>(),
                    _modelPrefabInstance.transform);*/
            }
            else
            {
                _actionClipPlay = null;
            }

            void InitModelInstance(GameObject instance)
            {
                var animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                var animancerComponent = instance.GetComponent<AnimancerComponent>();
                if (animancerComponent == null)
                {
                    animancerComponent = instance.AddComponent<AnimancerComponent>();
                }

                animancerComponent.Animator = animator;
                animator.applyRootMotion = true;
            }
        }
        private void ClickSaveConfigButton()
        {
            if (!_actionClip)
            {
                return;
            }

            /*SetNewestActionData(_actionClip);*/
            EditorUtility.SetDirty(_actionClip);
            AssetDatabase.SaveAssets();
        }
        private void HandleModelPrefabObjectFieldValueChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is GameObject gameObject)
            {
                _modelPrefab = gameObject;
            }
        }

        private void HandleConfigFileObjectFieldValueChanged(ChangeEvent<Object> value)
        {
            if (value.newValue is ActionClip actionClip && actionClip)
            {
                if (_actionClip == actionClip)
                {
                    return;
                }

                SelectedFrame = 0;
                _actionClip = actionClip;
                /*if (Selection.activeObject is ActionChannelInspectorSO ||
                    Selection.activeObject is ActionTrackInspectorSO)
                {
                    Selection.activeObject = null;
                }

                CreateConfigUI();*/
            }
            else
            {
                SelectedFrame = 0;
                _actionClip = null;
                /*if (Selection.activeObject is ActionChannelInspectorSO ||
                    Selection.activeObject is ActionTrackInspectorSO)
                {
                    Selection.activeObject = null;
                }

                DestroyConfigUI();*/
            }
        }
        #endregion
        
        /*public void CreateChannel_Track(ActionTrackType trackType)
        {
            // create channel(create track internal)
            var channel = new ChannelBase(trackType.ToString(), trackType, _channelList_uxml,
                new ActionTrackEditorUIData
                {
                    Name = trackType.ToString(),
                }
        }*/
        #region Channel相关

        private void InitChannelConfig()
        {
            // 设定事件绑定
            _listChannelList.makeItem = CreateChannelListItemView;
            _listChannelList.bindItem += BindChannelListItemView;
            _listChannelList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;   
        }
        private void DestroyChannelConfig()
        {
            _listChannelList.makeItem -= CreateChannelListItemView;
            _listChannelList.bindItem -= BindChannelListItemView;
            _listChannelList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;   
        }

        private void BindChannelListItemView(VisualElement arg1, int arg2)
        {
            var data = _listChannelList.itemsSource[arg2] as ChannelBase;
            var nameLabel = arg1.Q<Label>("LbChannelName");
            nameLabel.text = data.channelName;
        }


        private VisualElement CreateChannelListItemView()
        {
            var item =  _channelListItem_uxml.CloneTree();
            item.styleSheets.Add(_channelListItem_styleSheet);
            return item;
        }

        #endregion
        #region 其他
        private void HandlePlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredPlayMode:
                {
                    var activeScene = SceneManager.GetActiveScene();
                    if (activeScene.path == EditorScenePath)
                    {
                        // 停止编辑动作播放器
                        /*ActionClipPlay?.Stop();*/
                    }
                }
                    break;
            }
        }
        #endregion
        #region 视图类
        
        #endregion
        //工具类
        private void ScheduleTimer(float time, UnityAction action)
        {
            _timerScheduleTime = DateTime.Now;
            _timerTriggerTime = time;
            _onTimerTriggered = action;
        }
        
        #region action clip相关
        private void OnSelectionChange()
        {
            TryToGetActionConfig();
        }
        private void TryToGetActionConfig()
        {
            if (Selection.activeObject is ActionClip actionConfig)
            {
                if (_ofFileConfig.value != actionConfig)
                {
                    _ofFileConfig.value = actionConfig;
                }
            }
        }
        #endregion



        private void OnDestroy() {
            
        }
    }
}