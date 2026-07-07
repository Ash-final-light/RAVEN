using Tools.RecycleViewFramework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.RecycleViewFramework.Widgets.Dialog
{
    public class Dialog : View.View
    {
        private GameObject blockObj;
        protected bool canClickBlock;

        public override void OnStart()
        {
            base.OnStart();

            CreateBlock();
        }

        public override void OnStop()
        {
            base.OnStop();

            Destroy(blockObj);
        }

        private void CreateBlock()
        {
            blockObj = new GameObject("Block");
            blockObj.transform.SetParent(uiManager.GameUI.transform);
            blockObj.layer = LayerMask.NameToLayer("UI");
            blockObj.AddComponent<RectTransform>().Fill();
            blockObj.SetSortingOrder((int)ViewType - 1);

            var image = blockObj.AddComponent<Image>();
            ColorUtility.TryParseHtmlString("#00000099", out Color color);
            image.color = color;

            if (canClickBlock)
            {
                var button = blockObj.AddComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    Close();
                });
            }
        }
    }
}
