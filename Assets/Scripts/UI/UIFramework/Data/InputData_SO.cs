using System.Collections.Generic;
using UnityEngine;

namespace UI.UIFramework.Data
{
    [CreateAssetMenu(fileName = "InputData", menuName = "Scriptable/InputData")]

//如果你需要添加新的icon，遵循注释(step1-2)添加即可

    public class InputData_SO : ScriptableObject
    {
        public Sprite MoveKeyboard;
        public Sprite MoveGamePad;

        public Sprite NaviKeyboard;
        public Sprite NaviGamePad;

        public List<GamePadIcons> gamepadIconsList;
        public KeyboardIcons keyboardIcons;

        public Sprite GetIconSprite(string _path, InputType _type)
        {
            if (_type == InputType.Keyboard)
            {
                return keyboardIcons.GetSprite(_path);
            }
            else if (_type == InputType.Xbox || _type == InputType.PS)
            {
                GamePadIcons icons = gamepadIconsList.Find(i => i.inputType == _type);
                return icons.GetSprite(_path);
            }
            else
            {
                return null;
            }
        }
    }

    [System.Serializable]
    public struct GamePadIcons
    {
        public InputType inputType;

        [SerializeField] Sprite buttonSouth;
        [SerializeField] Sprite buttonNorth;
        [SerializeField] Sprite buttonEast;
        [SerializeField] Sprite buttonWest;
        [SerializeField] Sprite startButton;
        [SerializeField] Sprite selectButton;
        [SerializeField] Sprite leftTrigger;
        [SerializeField] Sprite rightTrigger;
        [SerializeField] Sprite leftShoulder;
        [SerializeField] Sprite rightShoulder;
        [SerializeField] Sprite dpad;
        [SerializeField] Sprite dpadUp;
        [SerializeField] Sprite dpadDown;
        [SerializeField] Sprite dpadLeft;
        [SerializeField] Sprite dpadRight;
        [SerializeField] Sprite leftStick;
        [SerializeField] Sprite rightStick;
        [SerializeField] Sprite leftStickPress;
        [SerializeField] Sprite rightStickPress;
        // [SerializeField] Sprite yourGamePadIcon; //step 1

        public Sprite GetSprite(string controlPath)
        {
            switch (controlPath)
            {
                case "<Gamepad>/buttonSouth": return buttonSouth;
                case "<Gamepad>/buttonNorth": return buttonNorth;
                case "<Gamepad>/buttonEast": return buttonEast;
                case "<Gamepad>/buttonWest": return buttonWest;
                case "<Gamepad>/start": return startButton;
                case "<Gamepad>/select": return selectButton;
                case "<Gamepad>/leftTrigger": return leftTrigger;
                case "<Gamepad>/rightTrigger": return rightTrigger;
                case "<Gamepad>/leftShoulder": return leftShoulder;
                case "<Gamepad>/rightShoulder": return rightShoulder;
                case "<Gamepad>/dpad": return dpad;
                case "<Gamepad>/dpad/up": return dpadUp;
                case "<Gamepad>/dpad/down": return dpadDown;
                case "<Gamepad>/dpad/left": return dpadLeft;
                case "<Gamepad>/dpad/right": return dpadRight;
                case "<Gamepad>/leftStick": return leftStick;
                case "<Gamepad>/rightStick": return rightStick;
                case "<Gamepad>/leftStickPress": return leftStickPress;
                case "<Gamepad>/rightStickPress": return rightStickPress;
                // case "<Gamepad>/yourPath": return yourGamePadIcon; //step 2
                //yourPath路径名可以这样找到：Input Control配置表-某个具体按键的右侧面板-点击Path右边的T按钮，即可显示路径
            }
            return null;
        }
    }


    [System.Serializable]
    public struct KeyboardIcons
    {
        public InputType inputType;

        [SerializeField] Sprite keyA;
        [SerializeField] Sprite keyB;
        [SerializeField] Sprite keyC;
        [SerializeField] Sprite keyD;
        [SerializeField] Sprite keyE;
        [SerializeField] Sprite keyF;
        [SerializeField] Sprite keyG;
        [SerializeField] Sprite keyH;
        [SerializeField] Sprite keyI;
        [SerializeField] Sprite keyJ;
        [SerializeField] Sprite keyK;
        [SerializeField] Sprite keyL;
        [SerializeField] Sprite keyM;
        [SerializeField] Sprite keyN;
        [SerializeField] Sprite keyO;
        [SerializeField] Sprite keyP;
        [SerializeField] Sprite keyQ;
        [SerializeField] Sprite keyR;
        [SerializeField] Sprite keyS;
        [SerializeField] Sprite keyT;
        [SerializeField] Sprite keyU;
        [SerializeField] Sprite keyV;
        [SerializeField] Sprite keyW;
        [SerializeField] Sprite keyX;
        [SerializeField] Sprite keyY;
        [SerializeField] Sprite keyZ;
        [SerializeField] Sprite key1;
        [SerializeField] Sprite key2;
        [SerializeField] Sprite key3;
        [SerializeField] Sprite key4;
        [SerializeField] Sprite key5;
        [SerializeField] Sprite key6;
        [SerializeField] Sprite key7;
        [SerializeField] Sprite key8;
        [SerializeField] Sprite key9;
        [SerializeField] Sprite key0;
        [SerializeField] Sprite keySpace;
        [SerializeField] Sprite keyEscape;
        [SerializeField] Sprite keySlash;
        [SerializeField] Sprite keyEnter;
        // [SerializeField] Sprite yourKeyIcon; //step 1

        public Sprite GetSprite(string controlPath)
        {
            switch (controlPath)
            {
                case "<Keyboard>/a": return keyA;
                case "<Keyboard>/b": return keyB;
                case "<Keyboard>/c": return keyC;
                case "<Keyboard>/d": return keyD;
                case "<Keyboard>/e": return keyE;
                case "<Keyboard>/f": return keyF;
                case "<Keyboard>/g": return keyG;
                case "<Keyboard>/h": return keyH;
                case "<Keyboard>/i": return keyI;
                case "<Keyboard>/j": return keyJ;
                case "<Keyboard>/k": return keyK;
                case "<Keyboard>/l": return keyL;
                case "<Keyboard>/m": return keyM;
                case "<Keyboard>/n": return keyN;
                case "<Keyboard>/o": return keyO;
                case "<Keyboard>/p": return keyP;
                case "<Keyboard>/q": return keyQ;
                case "<Keyboard>/r": return keyR;
                case "<Keyboard>/s": return keyS;
                case "<Keyboard>/t": return keyT;
                case "<Keyboard>/u": return keyU;
                case "<Keyboard>/v": return keyV;
                case "<Keyboard>/w": return keyW;
                case "<Keyboard>/x": return keyX;
                case "<Keyboard>/y": return keyY;
                case "<Keyboard>/z": return keyZ;
                case "<Keyboard>/1": return key1;
                case "<Keyboard>/2": return key2;
                case "<Keyboard>/3": return key3;
                case "<Keyboard>/4": return key4;
                case "<Keyboard>/5": return key5;
                case "<Keyboard>/6": return key6;
                case "<Keyboard>/7": return key7;
                case "<Keyboard>/8": return key8;
                case "<Keyboard>/9": return key9;
                case "<Keyboard>/0": return key0;
                case "<Keyboard>/space": return keySpace;
                case "<Keyboard>/escape": return keyEscape;
                case "<Keyboard>/slash": return keySlash;
                case "<Keyboard>/enter": return keyEnter;
                // case "<Keyboard>/yourPath": return yourKeyIcon; //step 2
                //yourPath可以这样找到：Input Control配置表-某个具体按键的右侧面板-点击Path右边的T按钮，即可显示路径
            }
            return null;
        }
    }
}