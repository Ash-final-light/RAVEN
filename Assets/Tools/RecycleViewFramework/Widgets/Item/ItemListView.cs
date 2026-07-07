using System.Collections.Generic;
using Common;
using Tools.CustomObject.Data;
using Tools.RecycleViewFramework.Utils;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Layout;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.RecycleViewFramework.Widgets.Item
{
    public class ItemListView : MonoBehaviour
    {
        [SerializeField] private ViewHolder itemViewHolder;

        [SerializeField] private Direction direction = Direction.Horizontal;
        public Direction Direction
        {
            get => direction;
            set
            {
                direction = value;
                if (recyclerView != null)
                {
                    recyclerView.Direction = direction;
                }
            }
        }

        [SerializeField] private Alignment alignment = Alignment.Center;
        public Alignment Alignment
        {
            get => alignment;
            set
            {
                alignment = value;
                if (recyclerView != null)
                {
                    recyclerView.Alignment = alignment;
                }
            }
        }

        [SerializeField] private Vector2 spacing = Vector2.zero;
        public Vector2 Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                if (recyclerView != null)
                {
                    recyclerView.Spacing = spacing;
                }
            }
        }

        [SerializeField] private Vector2 padding = Vector2.zero;
        public Vector2 Padding
        {
            get => padding;
            set
            {
                padding = value;
                if (recyclerView != null)
                {
                    recyclerView.Padding = padding;
                }
            }
        }

        [SerializeField] private bool scroll = true;
        public bool Scroll
        {
            get => scroll;
            set => scroll = value;
        }

        private RecyclerView.RecyclerView recyclerView;
        private Adapter<ObjectInfo> itemAdapter;

        private GameObject contentObj;

        private void Awake()
        {
            recyclerView = gameObject.AddComponent<RecyclerView.RecyclerView>();
            recyclerView.Direction = direction;
            recyclerView.Alignment = alignment;
            recyclerView.Spacing = spacing;
            recyclerView.Padding = padding;
            recyclerView.Scroll = scroll;
            recyclerView.Templates = new ViewHolder[1] { itemViewHolder };
            CreateContentObj();
            itemAdapter = new Adapter<ObjectInfo>(recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager());
            itemAdapter.SetOnItemClick(data =>
            {
                if (GameViewManager.UIManager.Open("ItemDetailsDialog", out ItemDetailsDialog dialog))
                {
                    dialog.SetItemData(data);
                }
            });
        }

        private void CreateContentObj()
        {
            contentObj = new("Content");
            contentObj.transform.SetParent(recyclerView.transform, false);
            contentObj.AddComponent<Image>();
            contentObj.AddComponent<Mask>().showMaskGraphic = false;
            contentObj.GetComponent<RectTransform>().Fill();
        }

        public void SetItemList(List<ObjectInfo> itemDatas)
        {
            itemAdapter.SetList(itemDatas);
        }

        public void SetItemData(ObjectInfo itemData)
        {
            itemAdapter.SetList(new List<ObjectInfo>() { itemData });
        }
    }
}
