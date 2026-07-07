using Common;
using Tools.Buff;
using Tools.Camera;
using Tools.Camera.Fight;
using Tools.Damage;
using Tools.InputHandle;
using VContainer;
using VContainer.Unity;

namespace Test
{
    public class TestCameraLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PlayerInputManager>();
            
            builder.RegisterComponentInHierarchy<MainCameraController>();
            builder.RegisterComponentInHierarchy<FightCamera>();
        }
    }
}