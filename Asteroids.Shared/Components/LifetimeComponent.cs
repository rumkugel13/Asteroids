using Kadro.ECS;

namespace Asteroids.Shared
{
    public class LifetimeComponent : IComponent
    {
        public float Lifetime;

        public LifetimeComponent(float lifetime)
        {
            this.Lifetime = lifetime;
        }
    }
}
