using Kadro;
using Kadro.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids.Shared
{
    public class SpriteAnimationRenderSystem : EntityDrawSystem, IUpdateSystem
    {
        public SpriteAnimationRenderSystem(EntityWorld entityWorld) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<SpriteAnimationComponent>() | 
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
                SpriteAnimationComponent animation = e.GetComponent<SpriteAnimationComponent>();

                System.Diagnostics.Debug.Assert(transform != null, "TransformComponent not found");
                System.Diagnostics.Debug.Assert(animation != null, "SpriteAnimationComponent not found");

                if (animation.Visible)
                {
                    spriteBatch.Draw(animation.Animation.Texture, transform.EntityToWorld(animation.Offset) * WindowSettings.UnitScale, animation.Animation.GetCurrentFrame(),
                        Color.White, transform.Rotation, animation.Animation.Origin, animation.Scale * WindowSettings.UnitScale, SpriteEffects.None, 0.8f);
                }
            }
        }

        public void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                SpriteAnimationComponent animation = e.GetComponent<SpriteAnimationComponent>();
                if (e.HasComponent<IntentComponent>())
                {
                    if (e.GetComponent<IntentComponent>().IntentManager.HasIntent(Intent.Accelerate))
                    {
                        animation.Visible = true;
                    }
                    else
                    {
                        animation.Visible = false;
                    }
                }
                animation.Animation.Update(elapsedSeconds);
            }
        }
    }
}
