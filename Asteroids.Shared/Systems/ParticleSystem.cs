using Kadro.ECS;
using Kadro.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Asteroids.Shared
{
    public class ParticleSystem : EntityUpdateSystem
    {
        private ParticleEngine particleEngine;

        public ParticleSystem(EntityWorld entityWorld, ParticleEngine particleEngine) : base(entityWorld, 
            entityWorld.ComponentManager.GetComponentId<IntentComponent>()|
            entityWorld.ComponentManager.GetComponentId<TransformComponent>() |
            entityWorld.ComponentManager.GetComponentId<ParticleSpawnComponent>())
        {
            this.particleEngine = particleEngine;
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                TransformComponent transform = e.GetComponent<TransformComponent>();
                ParticleSpawnComponent particles = e.GetComponent<ParticleSpawnComponent>();
                IntentComponent intent = e.GetComponent<IntentComponent>();

                System.Diagnostics.Debug.Assert(transform != null, "PositionComponent not found");
                System.Diagnostics.Debug.Assert(particles != null, "ParticleSpawnComponent not found");
                System.Diagnostics.Debug.Assert(intent != null, "IntentComponent not found");

                if (intent.IntentManager.HasIntent(Intent.Accelerate))
                {
                    this.particleEngine.SpawnParticles(particles.Amount, transform.EntityToWorld(particles.Location)
                       /* (transform.Position + VectorExtensions.RotateVector(particles.Location, transform.Rotation))*/, particles.Color, transform.Rotation + MathHelper.Pi, particles.AngularAperture);
                }
            }
        }
    }
}
