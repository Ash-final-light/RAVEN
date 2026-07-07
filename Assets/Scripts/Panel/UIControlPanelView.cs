using System.Collections.Generic;
using Common;
using NaiQiu.Framework.View;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.View;
using Tools.RecycleViewFramework.Widgets.RecyclerView;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Layout;

namespace Panel
{
    public class UIControlPanelView:View
    {
        private RecyclerView recyclerview;
        private Adapter<OpenViewHolderData> adapter;

        public override void OnCreate(UIManager uiManager)
        {
            base.OnCreate(uiManager);
            recyclerview = transform.Find("RecyclerView").GetComponent<RecyclerView>();
            adapter = new Adapter<OpenViewHolderData>(recyclerview);
            recyclerview.SetLayoutManager(new LinearLayoutManager());
            adapter.SetOnItemClick(data =>
                {
                    GameViewManager.UIManager.Open(data.viewName);
                    GameViewManager.UIManager.Close("UIControlPanelView");
                }
            );
            FillData();
        }

        private void FillData()
        {
            List<OpenViewHolderData> openViewHolderData = new()
            {
                new OpenViewHolderData
                {
                    spriteName = "Icon_Task_All", atlasName = "ItemIconAtlas", name = "任务",
                    viewName = "QuestView"
                },
                new OpenViewHolderData
                {
                    spriteName = "Icon_Props", atlasName = "ItemIconAtlas", name = "背包",
                    viewName = "ObjectPackageView"
                },
            };
            adapter.SetList(openViewHolderData);
        }
    }

    public class OpenViewHolderData
    {
        public string spriteName;
        public string atlasName;
        public string name;
        public string viewName;
    }
}