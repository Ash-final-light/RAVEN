using Common;
using Tools.Buff;
using Tools.Camera;
using Tools.Camera.Fight;
using Tools.Damage;
using Tools.InputHandle;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Test
{
    public class TestFightLifetimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<LuaManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<BuffManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();
            
            builder.RegisterComponentInHierarchy<PlayerInputManager>();
            builder.RegisterComponentInHierarchy<AlgorithmManager>();
            builder.RegisterComponentInHierarchy<Tools.Battle.BattleManager>();
            var controller = FindObjectOfType<TestStateController>();
            if (controller != null)
            {
                builder.RegisterComponent(controller);
            }
            else
            {
                Debug.LogWarning("[TestFightLifetimeScope] TestStateController not found. Skip controller registration for UI-only scene.");
            }

            builder.RegisterComponentInHierarchy<MainCameraController>();
            builder.RegisterComponentInHierarchy<FightCamera>();
        }
    }
}
