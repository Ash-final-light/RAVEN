namespace UI.UIFramework.Data
{
    public enum PanelName
    {
        None, PanelSample, PanelPreset, PanelMenu, PanelOption, PanelCredits, PanelPause
    }

    public enum SubPanelName
    {
        None, General, Graphics, Sound, Keyboard, Gamepad,Archive
    }

    public enum TitleName
    {
        None, StartGame, Option, Credits, Quit, CloseSubPanel, ClosePanel, Continue, GoTitle, Keyboard, Gamepad, ResetInput, Wait, Language, CompletePreset,
        GameTitle, General, Graphics, Sound, Test, Saved,Archive
    }
    

    public enum OptionName
    {
        None = 0,
        PresetProgress = 1,
        LevelProgress = 2,
        Lan = 3,
        Vibration = 4,
        Contrast = 5,
        FontSize = 6,
        MasterVolume = 7,
        Music = 8,
        SoundEffect = 9,
        Fullscreen = 10,
        FrameRate = 11,
        Resolution = 15
    }
    public enum SceneName
    {
        Menu,Daily
    }

    public enum InputType
    {
        Keyboard, Xbox, PS, Other
    }

    public enum DataStatus
    {
        NoPresetData, HasPresetData
    }

    public enum ManagerStatus
    {
        Loading, Started
    }

    public enum GameStatus
    {
        None, Loaded, Playing, Paused, Clear
    }

    public enum Language
    {
        English = 0, Chinese = 1, Japanese = 2
    }

    public enum OptionSetting
    {
        Off, On
    }

    public enum FontSize
    {
        Small, Medium, Large
    }
}
