using Kadro.ECS;

namespace Asteroids.Shared
{
    public class AccelerationComponent : IComponent
    {
        public float AccelerationFactor;

        public float RotationAccelerationFactor;

        public AccelerationComponent(float accelerationRate, float rotationAccelerationRate)
        {
            this.AccelerationFactor = accelerationRate;
            this.RotationAccelerationFactor = rotationAccelerationRate;
        }
    }
}
