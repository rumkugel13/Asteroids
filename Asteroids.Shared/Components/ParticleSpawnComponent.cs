using Kadro.ECS;
using Microsoft.Xna.Framework;
using System;

namespace Asteroids.Shared
{
    public class ParticleSpawnComponent : IComponent
    {
        public int Amount;

        public Color Color;

        public float AngularAperture;

        public Vector2 Location;

        public ParticleSpawnComponent(int amount, Vector2 location, Color color, float angularAperture)
        {
            this.Amount = amount;
            this.Color = color;
            this.AngularAperture = angularAperture;
            this.Location = location;
        }
    }
}
