using Daily;
using Daily.Archive;
using Package;
using Quest;
using Tools.CustomObject.Interface;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Sprite;
using Tools.RecycleViewFramework.View;
using VContainer;
using VContainer.Unity;

namespace Common
{
    public sealed class DailyLifetimeScope : LifetimeScope
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
            builder.Register<Tools.ReddotTree.ReddotTree>(Lifetime.Singleton);
            builder.Register<PackageObjectManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IPackageObjectManager>()
                .As<IPackageObjectProvider>();
            builder.Register<GameViewManager>(Lifetime.Singleton)
                .As<IStartable>();

            builder.Register<DailyPackageService>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<DailyWeekArchiveController>();
            builder.RegisterComponentInHierarchy<DailyNodeActionDispatcher>();
            builder.RegisterComponentInHierarchy<DailyBagIconButton>();
        }
    }
}
