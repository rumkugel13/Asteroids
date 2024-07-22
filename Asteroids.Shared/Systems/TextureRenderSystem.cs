using Kadro.ECS;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Asteroids.Shared
{
    public class TextureRenderSystem : EntityDrawSystem
    {
        Texture2D ringTexture;

        public TextureRenderSystem(EntityWorld entityWorld, ContentManager content) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<TextureComponent>() | 
            entityWorld.ComponentManager.GetComponentId<TransformComponent>())
        {
            ringTexture = content.Load<Texture2D>(GameConfig.Folders.Textures + "/ring-512");
        }

        public override void Draw(float elapsedSeconds, SpriteBatch spriteBatch)
        {
            //Parallel.ForEach(this.actives.Values, (e) =>
            //{

            //});

            foreach (Entity e in this.actives.Values)
            {
                TransformComponent transform = e.GetComponent<TransformComponent>();
                TextureComponent texture = e.GetComponent<TextureComponent>();

                System.Diagnostics.Debug.Assert(transform != null, "PositionComponent not found");
                System.Diagnostics.Debug.Assert(texture != null, "TextureComponent not found");

                Vector2 position = transform.Position * WindowSettings.UnitScale;
                Vector2 scale = texture.ScaleFactor * WindowSettings.UnitScale;

                spriteBatch.Draw(texture.Texture, position, null, Color.White, transform.Rotation, texture.Origin, scale, SpriteEffects.None, texture.LayerDepth);

                // HACK: draw the ring on top of the spaceship if shield component is available
                // TODO: let entities have multiple textures and render them according to their order (or just use layerDepth)
                if (e.HasComponent<ShieldComponent>())
                {
                    spriteBatch.Draw(ringTexture, position, null, Color.White, transform.Rotation, ringTexture.Bounds.Size.ToVector2() / 2f, scale, SpriteEffects.None, 0.8f);
                }
            }
        }
    }
}
