using Kadro.ECS;
using Kadro.Extensions;
using Kadro.Input;
using Microsoft.Xna.Framework;

namespace Asteroids.Shared
{
    public class TargetingSystem : EntityUpdateSystem
    {
        public TargetingSystem(EntityWorld entityWorld) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<TransformComponent>() |
            entityWorld.ComponentManager.GetComponentId<MotionComponent>() |
            entityWorld.ComponentManager.GetComponentId<TargetComponent>())
        {
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                TransformComponent transform = e.GetComponent<TransformComponent>();
                MotionComponent motion = e.GetComponent<MotionComponent>();
                TargetComponent target = e.GetComponent<TargetComponent>();

                System.Diagnostics.Debug.Assert(transform != null, "PositionComponent not found");
                System.Diagnostics.Debug.Assert(motion != null, "VelocityComponent not found");
                System.Diagnostics.Debug.Assert(target != null, "TargetComponent not found");

                if (EntityWorld.EntityManager.IsAlive(target.TargetId))
                {
                    TransformComponent targetTransform = this.EntityWorld.ComponentManager.GetComponent<TransformComponent>(target.TargetId);

                    if (targetTransform != null)
                    {
                        Vector2 delta = targetTransform.Position - transform.Position;
                        float destinationDirection = VectorExtensions.Vector2ToAngle(delta);
                        int accelFactor = 10;
                        float newRotation = MathExtensions.LerpAngle(MathHelper.WrapAngle(transform.Rotation), MathHelper.WrapAngle(destinationDirection), /*0.1f*/2 * elapsedSeconds);
                        transform.Rotation = newRotation;
                        motion.AddAcceleration(VectorExtensions.AngleToVector2(transform.Rotation) * GameConfig.Projectile.Velocity * accelFactor);
                        //motion.AddAngularAcceleration((destinationDirection - transform.Rotation) * 0.5f);
                    }
                }
                //else
                //{
                //    //target lost
                //    motion.AddAcceleration(VectorExtensions.AngleToVector2(transform.Rotation) * GameConfig.Projectile.Velocity * accelFactor);
                //}
            }
        }
    }
}
