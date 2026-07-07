using System.Collections.Generic;
using UI.UIFramework.Control;

namespace UI.UIFramework.Data
{
    public class OptionContent
    {
        public string[] optionValueTitleList;
        public string optionName;
        public List<string>[] optionValuesLanUI;

        public OptionContent(OptionBundle optionBundle)
        {
            optionName = optionBundle.OptionParent;
            optionValueTitleList = optionBundle.OptionChilds.ToArray();
            optionValuesLanUI = new List<string>[optionBundle.OptionChilds.Count];
            for (int i = 0; i < optionBundle.OptionChilds.Count; i++)
            {
                optionValuesLanUI[i] = 
                    UIDataManager.Instance.GetAllLanUIContent(optionBundle.OptionChilds[i]);
            }
        }

        public int GetOptionValuesLength()
        {
            return optionValuesLanUI.Length;
        }
        public string GetOptionValueTitleByValueIndex(int _index)
        {
            if (_index < 0 || _index >= optionValueTitleList.Length)
            {
                return string.Empty;
            }
            return optionValueTitleList[_index];
        }
        
        public string GetOptionLanSelectedValue(int _index,int _lanIndex)
        {
            if (_index < 0 || _index >= optionValuesLanUI.Length)
            {
                return string.Empty;
            }

            var values = optionValuesLanUI[_index];
            if (values == null || _lanIndex < 0 || _lanIndex >= values.Count)
            {
                return optionValueTitleList != null && _index < optionValueTitleList.Length
                    ? optionValueTitleList[_index]
                    : string.Empty;
            }

            return optionValuesLanUI[_index][_lanIndex];
        }
        
        public List<string> GetOptionLanUIByValueIndex(int _index)
        {
            if (_index < 0 || _index >= optionValuesLanUI.Length)
            {
                return new List<string>();
            }
            return optionValuesLanUI[_index];
        }
    }
}
