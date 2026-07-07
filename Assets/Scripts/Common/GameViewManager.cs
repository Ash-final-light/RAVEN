using NaiQiu.Framework.View;
using Package;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Sprite;
using Tools.RecycleViewFramework.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common
{
    public class GameViewManager : IStartable
    {
        public static UIManager UIManager;
        private static ResourceManager ResourceManager;
        public static SpriteManager SpriteManager;
        public static PackageObjectManager PackageObjectManager;
        public static Tools.ReddotTree.ReddotTree ReddotTree;

        [Inject]
        public GameViewManager(UIManager uiManager, ResourceManager resourceManager, SpriteManager spriteManager, 
            PackageObjectManager packageObjectManager, Tools.ReddotTree.ReddotTree reddotTree)
        {
            UIManager = uiManager;
            ResourceManager = resourceManager;
            SpriteManager = spriteManager;
            PackageObjectManager = packageObjectManager;
            ReddotTree = reddotTree;
            
            packageObjectManager.OnObjectGroupAdded += (parentId,group) =>
            {
                reddotTree.RegisterReddotNode(group.GroupId,true, group.Number);
                reddotTree.BindChildParent(group.GroupId,parentId);
                reddotTree.InvokeReddotChangeAction(parentId,group.Number);
                Debug.Log($"GameManager OnObjectGroupAdded {group.GroupId}, {group.Number}");
            };
            packageObjectManager.OnObjectGroupRemoved += (parentId,group) =>
            {
                reddotTree.RemoveReddotNode(group.GroupId);
                Debug.Log($"GameManager OnObjectGroupRemoved {group.GroupId}, {group.Number}");
            };
        }
        
        public void Start()
        {
            ResourceManager.OnResourceLoaded += () =>
            {
                /*Debug.Log("GameManager Start");
                UIManager.Open("DialogView");
                UIManager.Open("UIControlPanelView");
                UIManager.Open("QuestView");
                UIManager.Close("QuestView");*/
            };
        }
    }
}