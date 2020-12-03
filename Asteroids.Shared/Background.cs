using Kadro.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Kadro;

namespace Asteroids.Shared
{
    public class Background
    {
        IndexedList<Particle> particles;
        Texture2D particleTexture;
        Random random;
        Rectangle area;
        double density;

        public Background(Texture2D particleTexture, Rectangle area)
        {
            this.particleTexture = particleTexture;
            this.particles = new IndexedList<Particle>();
            this.random = new Random();
            this.area = area;
            this.density = 128d / (1280d * 720d);
        }

        public void Create()
        {
            this.particles.Clear();
            int particleCount = (int)Math.Ceiling(this.density * this.area.Width * this.area.Height);

            for (int i = 0; i < particleCount; i++)
            {
                Vector2 pos = new Vector2(this.area.X + (float)this.random.NextDouble() * this.area.Size.X, this.area.Y + (float)this.random.NextDouble() * this.area.Size.Y);
                float opacity = (float)this.random.NextDouble();
                float size = (float)this.random.NextDouble() * 1 / 3f;
                Particle p = new Particle(this.particleTexture, pos, Vector2.Zero, 0, 0, Color.White * opacity, size, float.MaxValue);
                this.particles.Add(p);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < this.particles.Count; i++)
            {
                this.particles[i].Draw(spriteBatch);
            }
        }
    }
}
