using Kadro.ECS;
using Microsoft.Xna.Framework;

namespace Asteroids.Shared
{
    public class MotionSystem : EntityUpdateSystem
    {
        public MotionSystem(EntityWorld entityWorld) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<TransformComponent>()|
            entityWorld.ComponentManager.GetComponentId<MotionComponent>())
        {
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                TransformComponent transform = e.GetComponent<TransformComponent>();
                MotionComponent motion = e.GetComponent<MotionComponent>();

                System.Diagnostics.Debug.Assert(transform != null, "TransformComponent not found");
                System.Diagnostics.Debug.Assert(motion != null, "VelocityComponent not found");

                motion.Velocity += (-motion.Velocity * motion.Drag) + motion.Acceleration * elapsedSeconds;

                if (motion.Velocity.LengthSquared() > motion.MaxVelocity * motion.MaxVelocity)
                {
                    motion.Velocity = Vector2.Normalize(motion.Velocity) * motion.MaxVelocity;
                }
                else if (motion.Velocity.LengthSquared() < 1 && motion.Acceleration == Vector2.Zero && motion.Velocity != Vector2.Zero)
                { 
                    // if object is not accelerating and still moving, set velocity to 0 if low enough
                    motion.Velocity = Vector2.Zero;
                }

                motion.AngularVelocity += (-motion.AngularVelocity * motion.AngularDrag) + motion.AngularAcceleration * elapsedSeconds;
                motion.AngularVelocity = MathHelper.Clamp(motion.AngularVelocity, -motion.MaxAngularVelocity, motion.MaxAngularVelocity);

                transform.Position += motion.Velocity * elapsedSeconds;
                transform.Rotation += motion.AngularVelocity * elapsedSeconds;

                motion.ResetForces();
            }
        }
    }
}
