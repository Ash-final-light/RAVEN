using System;
using System.Collections.Generic;
using Common;
using Framework.Core.Singleton;
using UI.UIFramework.Data;
using UnityEngine;

namespace UI.UIFramework.Control
{
    public class UIDataManager : MonoBehaviour, ISingleton
    {
        private static UIDataManager _instance;
        private const string LanguagePrefKey = "UIFramework.Language";
        private const string FontSizePrefKey = "UIFramework.FontSize";

        public LanUIContainer LanUIContainer =>
            GameApplication.Instance.ExcelBinaryManager.GetContainer<LanUIContainer>();

        public Language curLan;
        public OptionData_SO OptionDataSo;

        public event Action LanguageChanged;
        public event Action FontSizeChanged;

        private readonly Dictionary<string, OptionBundle> optionBundles = new();
        private readonly Dictionary<string, int> optionValues = new();
        private bool initialized;

        public static bool HasInstance => _instance != null;

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            EnsureInitialized();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            InitOptionBundle();
            LoadPersistedOptions();
            initialized = true;
        }

        public int GetOptionValue(string optionName)
        {
            EnsureInitialized();
            if (!optionValues.TryGetValue(optionName, out var value))
            {
                optionValues.Add(optionName, 0);
                return 0;
            }

            return value;
        }

        public void ApplyOptionValue(string optionName, int value)
        {
            EnsureInitialized();
            if (!Enum.TryParse(optionName, out OptionName option))
            {
                Debug.LogError($"Error: Option {optionName} Not Found!");
                return;
            }

            if (!optionBundles.ContainsKey(optionName) && CreateBuiltInOptionBundle(optionName) == null)
            {
                Debug.LogWarning($"OptionBundle {optionName} Not Found! Applying mapped setting without bundle.");
            }

            optionValues[optionName] = value;
            ApplyMappedSetting(option, value);
        }

        public OptionContent GetOptionContent(string optionName)
        {
            EnsureInitialized();
            if (!optionBundles.TryGetValue(optionName, out var bundle))
            {
                bundle = CreateBuiltInOptionBundle(optionName);
                if (bundle == null)
                {
                    return null;
                }
            }

            return new OptionContent(bundle);
        }

        public OptionBundle GetOptionBundle(string optionName)
        {
            EnsureInitialized();
            if (!optionBundles.TryGetValue(optionName, out var bundle))
            {
                bundle = CreateBuiltInOptionBundle(optionName);
            }

            return bundle;
        }

        public string GetOptionTitle(string optionName)
        {
            return LanUIContainer.Data.ContainsKey(optionName) ? GetLanUIContent(optionName) : optionName;
        }

        public int GetCurrentLanguageIndex()
        {
            return ((int)curLan) + 1;
        }

        public List<string> GetAllLanUIContent(string titleName)
        {
            EnsureInitialized();
            if (!LanUIContainer.Data.TryGetValue(titleName, out var content))
            {
                return new List<string>();
            }

            return new List<string>
            {
                content.TitleName,
                content.English,
                content.Chinese,
                content.Japanese
            };
        }

        public string GetLanUIContent(string titleName)
        {
            EnsureInitialized();
            if (!LanUIContainer.Data.TryGetValue(titleName, out var content))
            {
                return "Error: Not Found!";
            }

            return curLan switch
            {
                Language.English => content.English,
                Language.Chinese => content.Chinese,
                Language.Japanese => content.Japanese,
                _ => "Error: Language not supported!"
            };
        }

        public void SaveCurrentSettings()
        {
            PlayerPrefs.Save();
        }

        private void InitOptionBundle()
        {
            OptionBundleInfoContainer optionBundleInfoContainer =
                GameApplication.Instance.ExcelBinaryManager.GetContainer<OptionBundleInfoContainer>();
            foreach (var info in optionBundleInfoContainer.Data)
            {
                optionBundles[info.Value.OptionParent] = info.Value.ToOptionBundle();
            }
        }

        private void LoadPersistedOptions()
        {
            curLan = ReadLanguageFromPrefs();

            optionValues[nameof(OptionName.Lan)] = MapLanguageToOptionValue(curLan);
            optionValues[nameof(OptionName.FontSize)] = PlayerPrefs.GetInt(FontSizePrefKey, (int)FontSize.Medium);
            optionValues[nameof(OptionName.Music)] = Mathf.RoundToInt(
                GameApplication.Instance.GlobalSettingsDataManager.MusicVolume / 10f);
            optionValues[nameof(OptionName.SoundEffect)] = Mathf.RoundToInt(
                GameApplication.Instance.GlobalSettingsDataManager.SoundVolume / 10f);
            optionValues[nameof(OptionName.Fullscreen)] =
                GameApplication.Instance.GlobalSettingsDataManager.ScreenMode == ScreenMode.Windowed
                    ? (int)OptionSetting.Off
                    : (int)OptionSetting.On;
            optionValues[nameof(OptionName.Resolution)] =
                (int)GameApplication.Instance.GlobalSettingsDataManager.DisplayResolution;
            optionValues[nameof(OptionName.FrameRate)] =
                (int)GameApplication.Instance.GlobalSettingsDataManager.FrameRate;
        }

        private void ApplyMappedSetting(OptionName option, int value)
        {
            switch (option)
            {
                case OptionName.Lan:
                    curLan = MapOptionValueToLanguage(value);
                    PlayerPrefs.SetInt(LanguagePrefKey, (int)curLan);
                    LanguageChanged?.Invoke();
                    break;
                case OptionName.FontSize:
                    PlayerPrefs.SetInt(FontSizePrefKey, value);
                    FontSizeChanged?.Invoke();
                    break;
                case OptionName.Music:
                    GameApplication.Instance.GlobalSettingsDataManager.MusicVolume = value * 10f;
                    break;
                case OptionName.SoundEffect:
                    GameApplication.Instance.GlobalSettingsDataManager.SoundVolume = value * 10f;
                    break;
                case OptionName.Fullscreen:
                    GameApplication.Instance.GlobalSettingsDataManager.ScreenMode =
                        value == (int)OptionSetting.On
                            ? ScreenMode.ExclusiveFullScreen
                            : ScreenMode.Windowed;
                    break;
                case OptionName.Resolution:
                    GameApplication.Instance.GlobalSettingsDataManager.DisplayResolution = (DisplayResolution)value;
                    break;
                case OptionName.FrameRate:
                    GameApplication.Instance.GlobalSettingsDataManager.FrameRate = (FrameRate)value;
                    break;
            }
        }

        private static int MapSaturationToOptionValue(float saturation)
        {
            return Mathf.Clamp(Mathf.RoundToInt((saturation / 2f) * 10f), 0, 10);
        }

        private static float MapOptionValueToSaturation(int value)
        {
            return Mathf.Clamp01(value / 10f) * 2f;
        }

        private static int MapLanguageToOptionValue(Language language)
        {
            return (int)language;
        }

        private static Language MapOptionValueToLanguage(int value)
        {
            return value switch
            {
                0 => Language.English,
                1 => Language.Chinese,
                2 => Language.Japanese,
                3 => Language.Japanese,
                _ => Language.English
            };
        }

        private static Language ReadLanguageFromPrefs()
        {
            var savedValue = PlayerPrefs.GetInt(LanguagePrefKey, (int)Language.English);
            return savedValue switch
            {
                0 => Language.English,
                1 => Language.Chinese,
                2 => Language.Japanese,
                3 => Language.Japanese,
                _ => Language.English
            };
        }

        private OptionBundle CreateBuiltInOptionBundle(string optionName)
        {
            if (optionName == nameof(OptionName.Resolution))
            {
                return new OptionBundle
                {
                    OptionParent = optionName,
                    OptionChilds = new List<string>
                    {
                        "1920x1080",
                        "1280x960",
                        "1440x1080",
                        "2560x1440"
                    }
                };
            }

            return null;
        }

        public static UIDataManager Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = MonoSingletonProperty<UIDataManager>.GetInstance(true);
                return _instance;
            }
        }

        public void OnSingletonInit()
        {
            MonoSingletonProperty<UIDataManager>.InitSingleton();
        }

        public void Dispose()
        {
            MonoSingletonProperty<UIDataManager>.Dispose();
        }
    }
}
