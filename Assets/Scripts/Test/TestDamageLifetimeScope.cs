using Common;
using Tools.Buff;
using Tools.Damage;
using VContainer;
using VContainer.Unity;

namespace Test
{
    public class TestDamageLifetimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<LuaManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<BuffManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();

            builder.RegisterComponentInHierarchy<AlgorithmManager>();
            builder.RegisterComponentInHierarchy<Tools.Battle.BattleManager>();
           
        }
    }
}