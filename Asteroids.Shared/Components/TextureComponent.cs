using Kadro.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids.Shared
{
    public class TextureComponent : IComponent
    {
        public Texture2D Texture;

        public Vector2 Origin;

        public Vector2 ScaleFactor;

        public float LayerDepth;

        public TextureComponent(Texture2D texture, Vector2 desiredSize, float layerDepth)
        {
            this.Texture = texture;
            this.ScaleFactor = Vector2.One * MathHelper.Min(desiredSize.X / texture.Bounds.Size.X, desiredSize.Y / texture.Bounds.Size.Y);
            this.LayerDepth = layerDepth;
            this.Origin = this.Texture.Bounds.Size.ToVector2() / 2f;
        }

        public TextureComponent(Texture2D texture, Vector2 desiredSize, Vector2 origin, float layerDepth) : this(texture, desiredSize, layerDepth)
        {
            this.Origin = origin;
        }
    }
}
