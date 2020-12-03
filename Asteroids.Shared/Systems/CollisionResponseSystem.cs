using Kadro.Physics.Colliders;
using Kadro.ECS;
using Microsoft.Xna.Framework;
using System;
using Kadro;

namespace Asteroids.Shared
{
    public class CollisionResponseSystem : EntityUpdateSystem
    {
        private readonly EntityFactory entityFactory;
        private readonly SignalManager signalManager;
        //private readonly LayerManager layerManager;
        private readonly BitVectorManager<string> layerManager;

        public CollisionResponseSystem(EntityWorld entityWorld, EntityFactory entityFactory, SignalManager signalManager) : base(entityWorld, 
            entityWorld.ComponentManager.GetComponentId<CollidableComponent>())
        {
            this.entityFactory = entityFactory;
            this.signalManager = signalManager;
            this.layerManager = entityFactory.LayerManager;
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                CollidableComponent collidable = e.GetComponent<CollidableComponent>();
                System.Diagnostics.Debug.Assert(collidable != null, "CollidableComponent not found");

                for (int i = 0; i < collidable.Collisions.Count; i++)
                {
                    this.HandleCollision(e, collidable, collidable.Collisions[i]);
                }

                collidable.Collisions.Clear();
            }
        }

        private bool HandleCollision(Entity e, CollidableComponent collidable, CollidableComponent other)
        {
            long playerLayer = this.layerManager.GetBit("Player");
            long asteroidLayer = this.layerManager.GetBit("Asteroid");
            long projectileLayer = this.layerManager.GetBit("Projectile");
            long borderLayer = this.layerManager.GetBit("Border");

            if (collidable.CollisionLayer.HasLayer(playerLayer) && other.CollisionLayer.HasLayer(asteroidLayer))
            {
                return this.CheckAgeAndRespawn(e);
            }
            else if (collidable.CollisionLayer.HasLayer(projectileLayer) && other.CollisionLayer.HasLayer(asteroidLayer))
            {
                //this.ParticleEngine?.SpawnParticles(10, collidable.Collision.CollisionPoint, Color.White);
                return this.AddScoreAndDestroy(e);
            }
            else if (collidable.CollisionLayer.HasLayer(asteroidLayer) && other.CollisionLayer.HasLayer(projectileLayer))
            {
                return this.SplitAndOrDestroy(e);
            }
            else if (collidable.CollisionLayer.HasLayer(asteroidLayer) && other.CollisionLayer.HasLayer(borderLayer))
            {
                return this.RepellFromBorderInward(e, 100);
            }
            else if (collidable.CollisionLayer.HasLayer(playerLayer) && other.CollisionLayer.HasLayer(borderLayer))
            {
                return this.StopAtBorder(e, collidable, other);
            }

            return true;
        }

        private bool CheckAgeAndRespawn(Entity e)
        {
            RespawnComponent respawn = e.GetComponent<RespawnComponent>();
            LifeComponent life = e.GetComponent<LifeComponent>();
            if (respawn != null && life != null)
            {
                if (respawn.TimeSinceRespawn >= respawn.Timeout)
                {
                    respawn.TimeSinceRespawn = 0;
                    e.GetComponent<IntentComponent>().IntentManager.Initialize();
                    TransformComponent position = e.GetComponent<TransformComponent>();
                    MotionComponent motion = e.GetComponent<MotionComponent>();
                    position.Position = respawn.Location;
                    position.Rotation = 0;
                    motion.Velocity = Vector2.Zero;
                    motion.AngularVelocity = 0;
                    life.Lives--;
                    e.AddComponent(new ShieldComponent());
                    this.signalManager.AddSignal(SignalType.LifeLost, e.EntityId);
                    if (life.Lives < 0)
                    {
                        e.Destroy();
                        this.signalManager.AddSignal(SignalType.EntityDestroyed, e.EntityId);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool AddScoreAndDestroy(Entity e)
        {
            ParentComponent parent = e.GetComponent<ParentComponent>();
            if (parent != null && this.EntityWorld.EntityManager.IsAlive(parent.ParentId))
            {
                this.EntityWorld.ComponentManager.GetComponent<ScoreComponent>(parent.ParentId).Value++;
                this.signalManager.AddSignal(SignalType.PointScored, parent.ParentId);
            }

            e.Destroy();
            this.signalManager.AddSignal(SignalType.EntityDestroyed, e.EntityId);
            return true;
        }

        private bool SplitAndOrDestroy(Entity e)
        {
            TransformComponent transform = e.GetComponent<TransformComponent>();

            if (transform.Scale.X > GameConfig.Asteroid.MinScale)  // split
            {
                Vector2 newScale = Vector2.One * ((transform.Scale.X == 1.0) ?  GameConfig.Asteroid.Medium / GameConfig.Asteroid.Large : GameConfig.Asteroid.Small / GameConfig.Asteroid.Large);
                this.entityFactory.AddAsteroid(this.EntityWorld.EntityManager.CreateEntity(), transform.Position, transform.Rotation + ((float)Math.PI / 6f), newScale);
                this.entityFactory.AddAsteroid(this.EntityWorld.EntityManager.CreateEntity(), transform.Position, transform.Rotation - ((float)Math.PI / 6f), newScale);
            }

            e.Destroy();
            this.signalManager.AddSignal(SignalType.EntityDestroyed, e.EntityId);
            return true;
        }

        private bool RepellFromBorderInward(Entity e, float acceleration)
        {
            TransformComponent transform = e.GetComponent<TransformComponent>();
            MotionComponent motion = e.GetComponent<MotionComponent>();

            motion.AddAcceleration(Vector2.Normalize(GameConfig.PlayArea.Center.ToVector2() - transform.Position) * acceleration);

            return true;
        }

        private bool StopAtBorder(Entity e, CollidableComponent collidable, CollidableComponent other)
        {
            TransformComponent transform = e.GetComponent<TransformComponent>();
            MotionComponent motion = e.GetComponent<MotionComponent>();

            Vector2 penetrationVector = collidable.Collider.PenetrationVector(other.Collider);

            if (!(penetrationVector == Vector2.Zero))
            {
                transform.Position -= penetrationVector;
                Vector2 offsetDirection = Vector2.Normalize(penetrationVector);
                Vector2 offsetMagnitude = new Vector2(Math.Abs(motion.Velocity.X), Math.Abs(motion.Velocity.Y));
                motion.Velocity -= offsetDirection * offsetMagnitude;
            }

            return true;
        }
    }
}
