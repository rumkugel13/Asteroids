using Kadro.ECS;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Asteroids.Shared
{
    public class MinimapRenderSystem : EntityDrawSystem
    {
        Minimap minimap;

        public MinimapRenderSystem(EntityWorld entityWorld, Minimap minimap) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<TextureComponent>() | 
            entityWorld.ComponentManager.GetComponentId<TransformComponent>())
        {
            this.minimap = minimap;
        }

        public override void Draw(float elapsedSeconds, SpriteBatch spriteBatch)
        {
            this.minimap.Clear();

            foreach (Entity e in this.actives.Values)
            {
                TransformComponent transform = e.GetComponent<TransformComponent>();
                TextureComponent texture = e.GetComponent<TextureComponent>();
                ScoreComponent score = e.GetComponent<ScoreComponent>();

                System.Diagnostics.Debug.Assert(transform != null, "PositionComponent not found");
                System.Diagnostics.Debug.Assert(texture != null, "TextureComponent not found");

                Vector2 position = transform.Position;

                if (score != null)
                    this.minimap.Add(position, Color.LightBlue);
                else
                    this.minimap.Add(position, Color.Orange);
            }
        }
    }
}
