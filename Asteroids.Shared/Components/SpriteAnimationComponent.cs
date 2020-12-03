using Kadro.ECS;
using Microsoft.Xna.Framework;
using Kadro;

namespace Asteroids.Shared
{
    public class SpriteAnimationComponent : IComponent
    {
        public SpriteAnimation Animation;

        public float Scale;
        public Vector2 Offset;
        public bool Visible;

        public SpriteAnimationComponent(SpriteAnimation animation, float scale, Vector2 offset)
        {
            this.Animation = animation;
            this.Scale = scale;
            this.Offset = offset;
        }
    }
}
