using Kadro.ECS;
using Microsoft.Xna.Framework;

namespace Asteroids.Shared
{
    public class RespawnComponent : IComponent
    {
        public Vector2 Location;

        public float Timeout { get; }

        public float TimeSinceRespawn;

        public RespawnComponent(Vector2 location, float timeout)
        {
            this.Location = location;
            this.Timeout = timeout;
            this.TimeSinceRespawn = 0;
        }
    }
}
