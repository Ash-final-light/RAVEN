using NaiQiu.Framework.View;
using Package;
using Tools.Buff;
using Tools.CustomObject.Interface;
using Tools.Dialog;
using Tools.Quest;
using Tools.Quest.Progression;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Sprite;
using Tools.RecycleViewFramework.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ResourceManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();

            builder.Register<SpriteManager>(resolver =>
            {
                var resourceManager = resolver.Resolve<ResourceManager>();
                return new SpriteManager(resourceManager.SpriteAtlas);
            }, Lifetime.Singleton);

            builder.Register<UIManager>(Lifetime.Singleton);
            builder.Register<DialogTreeExecutor>(Lifetime.Singleton);
            builder.Register<Tools.ReddotTree.ReddotTree>(Lifetime.Singleton);

            builder.Register<PackageObjectManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IPackageObjectManager>()
                .As<IPackageObjectProvider>();
            builder.Register<LuaManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<BuffManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();

            builder.Register<GameViewManager>(Lifetime.Singleton)
                .As<IStartable>();

            builder.Register<IPlayerProgressionService, PlayerProgressionService>(Lifetime.Singleton);

            QuestManager questManager = Object.FindFirstObjectByType<QuestManager>();
            if (questManager != null)
            {
                builder.RegisterComponent(questManager);
            }

            builder.RegisterComponentInHierarchy<AbManager>();
            builder.RegisterComponentInHierarchy<TestMono>();
            builder.RegisterComponentInHierarchy<Tools.Battle.BattleManager>();
        }
    }
}
