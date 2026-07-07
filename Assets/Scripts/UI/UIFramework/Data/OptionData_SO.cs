using System.Collections.Generic;
using UnityEngine;

namespace UI.UIFramework.Data
{
    [CreateAssetMenu(fileName = "OptionData", menuName = "Scriptable/OptionData")]

    public class OptionData_SO : ScriptableObject
    {
        [SerializeField] List<SettingFont> fontList;
        public ColorType colorType;

        public SettingFont GetFontSize(int value)
        {
            FontSize size = (FontSize)value;
            return fontList.Find(i => i.fontSize == size);
        }

        [System.Serializable]
        public struct SettingFont
        {
            public FontSize fontSize;
            public int titleSize;
            public int subSize;
        }

        [System.Serializable]
        public struct ColorType
        {
            public Color light;
            public Color dark;
        }

    }
}