using Kadro.ECS;

namespace Asteroids.Shared
{
    public class LifetimeSystem : EntityUpdateSystem
    {
        private SignalManager signalManager;

        public LifetimeSystem(EntityWorld entityWorld, SignalManager signalManager) : base(entityWorld, 
            entityWorld.ComponentManager.GetComponentId<LifetimeComponent>())
        {
            this.signalManager = signalManager;
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                LifetimeComponent lifetime = e.GetComponent<LifetimeComponent>();
                System.Diagnostics.Debug.Assert(lifetime != null, "LifetimeComponent not found");

                if (lifetime.Lifetime < 0)
                {
                    e.Destroy();
                    this.signalManager.AddSignal(SignalType.EntityDestroyed, e.EntityId);
                }
                else
                {
                    lifetime.Lifetime -= elapsedSeconds;
                }
            }
        }
    }
}
