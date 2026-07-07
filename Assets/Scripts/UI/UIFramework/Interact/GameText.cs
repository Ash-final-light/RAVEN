using UI.UIFramework.Control;
using UI.UIFramework.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIFramework.Interact
{
    public class GameText : MonoBehaviour
    {
        [SerializeField] TitleName title_name;
        [SerializeField] bool isTitle;
        [SerializeField] bool darkColor;

        private Text txt;

        private void Awake()
        {
            txt = gameObject.GetComponent<Text>();
        }

        private void OnEnable()
        {
            UIDataManager.Instance.LanguageChanged += ShowText;
            UIDataManager.Instance.FontSizeChanged += ShowText;
            ShowText();
        }

        private void OnDisable()
        {
            if (!UIDataManager.HasInstance)
            {
                return;
            }

            UIDataManager.Instance.LanguageChanged -= ShowText;
            UIDataManager.Instance.FontSizeChanged -= ShowText;
        }

        public void ShowText()
        {
            if (!txt)
            {
                return;
            }

            if (title_name != TitleName.None)
            {
                txt.text = UIDataManager.Instance.GetLanUIContent(title_name.ToString());
            }

            txt.color = darkColor
                ? UIDataManager.Instance.OptionDataSo.colorType.dark
                : UIDataManager.Instance.OptionDataSo.colorType.light;

            int curFontSize = UIDataManager.Instance.GetOptionValue(nameof(OptionName.FontSize));
            var fontSetting = UIDataManager.Instance.OptionDataSo.GetFontSize(curFontSize);
            txt.fontSize = isTitle ? fontSetting.titleSize : fontSetting.subSize;
        }
    }
}
