using Kadro.ECS;

namespace Asteroids.Shared
{
    public class RespawnTimeoutSystem : EntityUpdateSystem
    {
        public RespawnTimeoutSystem(EntityWorld entityWorld) : base(entityWorld, 
            entityWorld.ComponentManager.GetComponentId<RespawnComponent>())
        {
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                RespawnComponent respawn = e.GetComponent<RespawnComponent>();
                System.Diagnostics.Debug.Assert(respawn != null, "RespawnComponent not found");

                if (respawn.TimeSinceRespawn >= respawn.Timeout && e.ContainsComponents(EntityWorld.ComponentManager.GetComponentId<ShieldComponent>()))
                {
                    e.RemoveComponent<ShieldComponent>();
                }
                else
                {
                    respawn.TimeSinceRespawn += elapsedSeconds;
                }
            }
        }
    }
}
