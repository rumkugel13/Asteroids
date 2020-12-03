using Kadro.ECS;
using Microsoft.Xna.Framework;

namespace Asteroids.Shared
{
    public class ShootSystem : EntityUpdateSystem
    {
        private EntityFactory entityFactory;

        public ShootSystem(EntityWorld entityWorld, EntityFactory entityFactory) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<GunComponent>() |
            entityWorld.ComponentManager.GetComponentId<TransformComponent>() |
            entityWorld.ComponentManager.GetComponentId<IntentComponent>())
        {
            this.entityFactory = entityFactory;
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                TransformComponent transform = e.GetComponent<TransformComponent>();
                GunComponent gun = e.GetComponent<GunComponent>();
                IntentComponent intent = e.GetComponent<IntentComponent>();

                System.Diagnostics.Debug.Assert(transform != null, "PositionComponent not found");
                System.Diagnostics.Debug.Assert(gun != null, "GunComponent not found");
                System.Diagnostics.Debug.Assert(intent != null, "IntentComponent not found");

                if ((gun.TimeSinceShot > gun.Cooldown) && intent.IntentManager.HasIntent(Intent.Shoot))
                {
                    gun.TimeSinceShot = 0;
                    Entity newEntity = this.EntityWorld.EntityManager.CreateEntity();
                    this.entityFactory.AddProjectile(newEntity, transform.EntityToWorld(gun.OffsetFromParent)
                       /* transform.Position + VectorExtensions.RotateVector(gun.OffsetFromParent, transform.Rotation)*/, transform.Rotation);
                    newEntity.GetComponent<ParentComponent>().ParentId = e.EntityId;
                }
                else if ((gun.TimeSinceShot > gun.Cooldown) && intent.IntentManager.HasIntent(Intent.ShootMissile))
                {
                    gun.TimeSinceShot = 0;
                    Entity newEntity = this.EntityWorld.EntityManager.CreateEntity();
                    this.entityFactory.AddMissile(newEntity, transform.EntityToWorld(gun.OffsetFromParent), transform.Rotation);
                    newEntity.GetComponent<ParentComponent>().ParentId = e.EntityId;

                    foreach (Entity entity in EntityWorld.EntityManager.GetEntities())
                    {
                        if (entity.GetComponent<TagComponent>().Tag.Equals(GameConfig.Asteroid.Tag))
                        {
                            newEntity.GetComponent<TargetComponent>().TargetId = entity.EntityId;
                            break;
                        }
                    }
                }
                else
                {
                    gun.TimeSinceShot += elapsedSeconds;
                }
            }
        }
    }
}
