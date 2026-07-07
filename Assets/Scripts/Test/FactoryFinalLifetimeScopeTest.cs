using Common;
using Package;
using Quest.GamePlay;
using Tools.Buff;
using Tools.Camera;
using Tools.Camera.Fight;
using Tools.CustomObject.Interface;
using Tools.Dialog;
using Tools.InputHandle;
using Tools.Quest;
using Tools.Quest.Progression;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Sprite;
using Tools.RecycleViewFramework.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Test
{
    public sealed class FactoryFinalLifetimeScopeTest : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterApplicationServices(builder);
            RegisterFightServices(builder);
            RegisterQuestServices(builder);
        }

        private static void RegisterApplicationServices(IContainerBuilder builder)
        {
            builder.Register<ResourceManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();

            builder.Register<SpriteManager>(resolver =>
            {
                var resourceManager = resolver.Resolve<ResourceManager>();
                return new SpriteManager(resourceManager.SpriteAtlas);
            }, Lifetime.Singleton).AsSelf();

            builder.Register<UIManager>(Lifetime.Singleton).AsSelf();
            builder.Register<DialogTreeExecutor>(Lifetime.Singleton).AsSelf();
            builder.Register<Tools.ReddotTree.ReddotTree>(Lifetime.Singleton).AsSelf();

            builder.Register<PackageObjectManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IPackageObjectManager>()
                .As<IPackageObjectProvider>();

            builder.Register<GameViewManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();
        }

        private void RegisterFightServices(IContainerBuilder builder)
        {
            builder.Register<LuaManager>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<BuffManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();

            builder.RegisterComponentInHierarchy<AlgorithmManager>();
            builder.RegisterComponentInHierarchy<Tools.Battle.BattleManager>();

            var controller = FindObjectOfType<TestStateController>();
            if (controller != null)
            {
                builder.RegisterComponent(controller);
            }
            else
            {
                Debug.LogWarning($"[{nameof(FactoryFinalLifetimeScopeTest)}] TestStateController not found. Skip controller registration.");
            }

            builder.RegisterComponentInHierarchy<MainCameraController>();
            //builder.RegisterComponentInHierarchy<FightCamera>();
        }

        private static void RegisterQuestServices(IContainerBuilder builder)
        {
            builder.Register<IPlayerProgressionService, PlayerProgressionService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<QuestManager>().AsSelf();
            builder.RegisterComponentInHierarchy<QuestVictoryViewListener>().AsSelf();
        }
    }
}
