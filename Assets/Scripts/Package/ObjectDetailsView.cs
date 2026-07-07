using Common;
using NaiQiu.Framework.View;
using TMPro;
using Tools.CustomObject.Data;
using Tools.RecycleViewFramework.Load;
using UnityEngine;
using UnityEngine.UI;

public class ObjectDetailsView : MonoBehaviour
{
    private Image iconImage;
    private TMP_Text nameText;
    private TMP_Text typeText;
    private TMP_Text contentText;
    private TMP_Text priceText;

    private void Awake()
    {
        nameText = transform.Find("Title/NameText").GetComponent<TMP_Text>();
        iconImage = transform.Find("Sub/IconImage").GetComponent<Image>();
        priceText = transform.Find("Sub/PriceText").GetComponent<TMP_Text>();
        typeText = transform.Find("Sub/TypeText").GetComponent<TMP_Text>();
        contentText = transform.Find("ContentText").GetComponent<TMP_Text>();
    }

    public void SetItemData(ObjectInfo objectInfo)
    {
        if (objectInfo == null)
            return;
        iconImage.sprite = GameViewManager.SpriteManager.GetSprite(objectInfo.Thumbnail);
        nameText.text = objectInfo.Name;
        typeText.text = objectInfo.Type;
        contentText.text = objectInfo.Introduction;
        priceText.text = objectInfo.Price.ToString();
    }
}
