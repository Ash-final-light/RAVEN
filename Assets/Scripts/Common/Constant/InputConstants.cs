

namespace Common.Constant
{
    public static partial class InputConstants
    {
        public const string GamePlayMap = "Player";
        public const string UIMap = "UI";
        
        public const string OpenMenu = "OpenMenu";
        public const string OpenTask = "OpenTask";
        public const string Move = "Move";
        
        public const string Cancel = "Cancel";
        public const string Click = "Click";
        public const string Navigate = "Navigate";
        
        public const string AnyGamepad = "AnyGamepad";
        public const string AnyGamepadUI = "AnyGamepadUI";
        public const string AnyKey = "AnyKey";
        public const string AnyKeyUI = "AnyKeyUI";


        public const string KeyboardDevice = "Keyboard&Mouse";
        public const string GamepadDevice = "Gamepad";
        
        public const string DualShockGamepadDevice = "DualShock";
        public const string DualSenseGamepadDevice = "DualSense";
    }
    public static partial class InputConstants
    {
        public const string SkillBall0 = "SkillBall0";
        public const string SkillBall1 = "SkillBall1";
        public const string SkillBall2 = "SkillBall2";
        public const string SkillBall3 = "SkillBall3";
        public const string SkillBall4 = "SkillBall4";
        public const string SkillBall5 = "SkillBall5";
        public const string SkillBall6 = "SkillBall6";
        public const string SkillBall7 = "SkillBall7";
        
        public const string PlayerMove = "PlayerMove";
        public const string PlayerAttack = "PlayerAttack";
        public const string PlayerSlide = "PlayerSlide";
        public const string PlayerLook = "PlayerLook";
        
        public const string PlayerInteract = "PlayerInteract";
        public const string PlayerLock =  "PlayerLock";
        public const string PlayerUltimate = "PlayerUltimate";
    }

    /// <summary>
    /// 当前仅针对Alpha角色，后续看情况是否需要扩展
    /// </summary>
    public static class AnimActionConstants
    {
        public const string RunStart = "run_start";
        public const string RunStop = "run_stop";
        public const string Run = "run";
        
        public const string idle01 =  "idle01";
        public const string Attack01 = "attack01";
        public const string Attack02 = "attack02";
        public const string Attack03 = "attack03";
        public const string Attack04 = "attack04";
        public const string Attack05 = "attack05";
        
        public const string SlideBehind = "slide_behind";
        public const string SlideFront = "slide_front";
        
        public const string ComboWindow = "combo_window";
        
        public const string hit01 = "hit01";
    }
    
    public static class ComboBlackboardConstants
    {
        public const string ComboWindow = "combo_window";
    }
    
}