using Kadro.ECS;
using Microsoft.Xna.Framework;

namespace Asteroids.Shared
{
    public class GunComponent : IComponent
    {
        public float Cooldown;

        public float TimeSinceShot;

        public Vector2 OffsetFromParent;

        public GunComponent(float cooldown, Vector2 offset)
        {
            this.Cooldown = cooldown;
            this.TimeSinceShot = cooldown;
            this.OffsetFromParent = offset;
        }
    }
}
