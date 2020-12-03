using Kadro.ECS;
using Kadro.Extensions;

namespace Asteroids.Shared
{
    public class MotionControlSystem : EntityUpdateSystem
    {
        public MotionControlSystem(EntityWorld entityWorld) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<TransformComponent>() |
            entityWorld.ComponentManager.GetComponentId<MotionComponent>() |
            entityWorld.ComponentManager.GetComponentId<AccelerationComponent>() |
            entityWorld.ComponentManager.GetComponentId<IntentComponent>())
        {
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                TransformComponent transform = e.GetComponent<TransformComponent>();
                MotionComponent motion = e.GetComponent<MotionComponent>();
                AccelerationComponent acceleration = e.GetComponent<AccelerationComponent>();
                IntentComponent intent = e.GetComponent<IntentComponent>();

                System.Diagnostics.Debug.Assert(transform != null, "PositionComponent not found");
                System.Diagnostics.Debug.Assert(motion != null, "VelocityComponent not found");
                System.Diagnostics.Debug.Assert(acceleration != null, "MotionComponent not found");
                System.Diagnostics.Debug.Assert(intent != null, "IntentComponent not found");

                if (intent.IntentManager.HasIntent(Intent.Accelerate))
                {
                    // add force to center of object; 
                    // TODO: add rocketengine/boostercomponent with offset from origin
                    motion.AddAcceleration(VectorExtensions.AngleToVector2(transform.Rotation) * acceleration.AccelerationFactor);
                }

                if (intent.IntentManager.HasIntent(Intent.Decelerate))
                {
                    motion.Drag = 0.05f;
                }
                else
                {
                    motion.Drag = 0.005f; //stop after a while
                }

                if (intent.IntentManager.HasIntent(Intent.RotateLeft))
                {
                    motion.AngularDrag = 0f;
                    motion.AddAngularAcceleration(-acceleration.RotationAccelerationFactor);
                }

                if (intent.IntentManager.HasIntent(Intent.RotateRight))
                {
                    motion.AngularDrag = 0f;
                    motion.AddAngularAcceleration(acceleration.RotationAccelerationFactor);
                }

                if (!(intent.IntentManager.HasIntent(Intent.RotateLeft) || intent.IntentManager.HasIntent(Intent.RotateRight)))
                {
                    motion.AngularDrag = 0.75f;
                }
            }
        }
    }
}
