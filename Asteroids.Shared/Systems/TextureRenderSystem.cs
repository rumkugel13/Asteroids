using Kadro.ECS;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids.Shared
{
    public class TextureRenderSystem : EntityDrawSystem
    {
        public TextureRenderSystem(EntityWorld entityWorld) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<TextureComponent>() | 
            entityWorld.ComponentManager.GetComponentId<TransformComponent>())
        {

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
                    Texture2D ringTexture = Assets.Get<Texture2D>(GameConfig.Folders.Textures, "ring-512");
                    spriteBatch.Draw(ringTexture, position, null, Color.White, transform.Rotation, ringTexture.Bounds.Size.ToVector2() / 2f, scale, SpriteEffects.None, 0.8f);
                }
            }
        }
    }
}
