using Kadro.Physics.Colliders;
using Kadro.ECS;
using Kadro.Extensions;
using Kadro;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Asteroids.Shared
{
    public class EntityFactory
    {
        private EntityWorld entityWorld;
        //public LayerManager LayerManager { get; }
        public BitVectorManager<string> LayerManager;
        private Random rnd;

        public EntityFactory(EntityWorld entityWorld)
        {
            rnd = new Random();
            this.entityWorld = entityWorld;
            //this.LayerManager = new LayerManager();
            this.LayerManager = new BitVectorManager<string>();
            this.LayerManager.Add("None");
            //this.LayerManager.AddLayer(GameConfig.Spaceship.CollisionLayer);
            //this.LayerManager.AddLayer(GameConfig.Projectile.CollisionLayer);
            //this.LayerManager.AddLayer(GameConfig.Asteroid.CollisionLayer);
            //this.LayerManager.AddLayer(GameConfig.Border.CollisionLayer);
        }

        public virtual void AddAsteroid(Entity e, Vector2 position, float rotation, Vector2 scale)
        {
            e.AddComponent(new TransformComponent(position, rotation, scale));
            e.AddComponent(new MotionComponent(VectorExtensions.AngleToVector2(rotation) * GameConfig.Asteroid.Velocity, ((rotation % 2 * Math.PI) - Math.PI) < 0 ? -GameConfig.Asteroid.AngularVelocity : +GameConfig.Asteroid.AngularVelocity, GameConfig.Asteroid.Velocity, GameConfig.Asteroid.AngularVelocity));
            e.AddComponent(new CollidableComponent(this.LayerManager.GetBit(GameConfig.Asteroid.CollisionLayer),
                this.LayerManager.GetBit(GameConfig.Spaceship.CollisionLayer) | this.LayerManager.GetBit(GameConfig.Projectile.CollisionLayer),
                new CircleCollider(GameConfig.Asteroid.Large * scale.X / 2f), e.EntityId));
            e.AddComponent(new TagComponent(GameConfig.Asteroid.Tag));
        }

        public virtual void AddProjectile(Entity e, Vector2 position, float rotation)
        {
            e.AddComponent(new TransformComponent(position, rotation, Vector2.One));
            e.AddComponent(new MotionComponent(Vector2.Transform(-Vector2.UnitY, Matrix.CreateRotationZ(rotation)) *
                /*VectorExtensions.AngleToVector2(rotation) * */ GameConfig.Projectile.Velocity, 0, GameConfig.Projectile.Velocity, 0));
            e.AddComponent(new CollidableComponent(this.LayerManager.GetBit(GameConfig.Projectile.CollisionLayer), 
                this.LayerManager.GetBit(GameConfig.Asteroid.CollisionLayer), 
                new CircleCollider(GameConfig.Projectile.Size / 2f), e.EntityId));
            e.AddComponent(new ParentComponent(0));
            e.AddComponent(new LifetimeComponent(GameConfig.Projectile.Lifetime));
            e.AddComponent(new TagComponent(GameConfig.Projectile.Tag));
        }

        public virtual void AddSpaceship(Entity e, Vector2 position, float rotation, IntentManager intentManager)
        {
            e.AddComponent(new TransformComponent(position, rotation, Vector2.One));
            e.AddComponent(new MotionComponent(Vector2.Zero, 0, GameConfig.Spaceship.MaxVelocity, GameConfig.Spaceship.MaxAngularVelocity));
            e.AddComponent(new ScoreComponent(GameConfig.Spaceship.Score));
            e.AddComponent(new LifeComponent(GameConfig.Spaceship.Lives));
            e.AddComponent(new RespawnComponent(position, GameConfig.Spaceship.RespawnTimeout));
            e.AddComponent(new ShieldComponent());
            e.AddComponent(new IntentComponent(intentManager));
            e.AddComponent(new GunComponent(GameConfig.Spaceship.GunCooldown, GameConfig.Spaceship.GunPosition));
            e.AddComponent(new AccelerationComponent(GameConfig.Spaceship.AccelerationRate, GameConfig.Spaceship.AngularAccelerationRate));
            e.AddComponent(new TagComponent(GameConfig.Spaceship.Tag));

            if (GameConfig.Spaceship.DefaultTexture == "spaceship_type_1")
            {
                // hack: use hardcoded values for now
                Vector2[] vertices = GameConfig.Spaceship.PolygonCollider;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] *= GameConfig.Spaceship.Size;   //HACK: apply scale to all vertices for originalvertices in polygon, to match size and scale of entity
                }

                e.AddComponent(new CollidableComponent(this.LayerManager.GetBit(GameConfig.Spaceship.CollisionLayer),
                    this.LayerManager.GetBit(GameConfig.Asteroid.CollisionLayer),
                    new PolygonCollider(vertices), e.EntityId));
            }
            else
            {
                e.AddComponent(new CollidableComponent(this.LayerManager.GetBit(GameConfig.Spaceship.CollisionLayer),
                    this.LayerManager.GetBit(GameConfig.Asteroid.CollisionLayer),
                    new CircleCollider(GameConfig.Spaceship.Size / 2f), e.EntityId));
            }
        }

        public virtual void AddBorder(Entity e, Vector2 position, Vector2 size)
        {
            e.AddComponent(new TransformComponent(position + size / 2f, 0, Vector2.One));
            e.AddComponent(new CollidableComponent(this.LayerManager.GetBit(GameConfig.Border.CollisionLayer), 
                this.LayerManager.GetBit(GameConfig.Asteroid.CollisionLayer) | this.LayerManager.GetBit(GameConfig.Spaceship.CollisionLayer), 
                new RectangleCollider(size.ToPoint()), e.EntityId));
            e.AddComponent(new TagComponent(GameConfig.Border.Tag));
        }

        public virtual void AddMissile(Entity e, Vector2 position, float rotation)
        {
            e.AddComponent(new TransformComponent(position, rotation, Vector2.One));
            e.AddComponent(new MotionComponent(
                VectorExtensions.AngleToVector2(rotation) * GameConfig.Projectile.Velocity,
                0,
                GameConfig.Projectile.Velocity,
                GameConfig.Spaceship.MaxAngularVelocity));
            e.AddComponent(new TagComponent(GameConfig.Projectile.Tag));
            e.AddComponent(new CollidableComponent(this.LayerManager.GetBit(GameConfig.Projectile.CollisionLayer),
                this.LayerManager.GetBit(GameConfig.Asteroid.CollisionLayer),
                new CircleCollider(GameConfig.Projectile.Size / 2f), e.EntityId));
            e.AddComponent(new ParentComponent(0));
            e.AddComponent(new LifetimeComponent(GameConfig.Projectile.Lifetime * 10));
            e.AddComponent(new TargetComponent(0));
            // add explosioncomponent, that destroys asteroids in a small range after first impact
            // add animation, that shows the explosion
        }

        public void SpawnAsteroids(Rectangle spawnArea, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                this.CreateAsteroid(spawnArea);
            }
        }

        public void CreateAsteroid(Rectangle spawnArea)
        {
            this.AddAsteroid(this.entityWorld.EntityManager.CreateEntity(), new Vector2(
                   spawnArea.X + spawnArea.Size.X * (float)this.rnd.NextDouble(),
                   spawnArea.Y + spawnArea.Size.Y * (float)this.rnd.NextDouble()), (float)this.rnd.NextDouble() * MathHelper.TwoPi, Vector2.One);
        }

        public void CreateBorder()
        {
            int thickness = (int)Math.Min((GameConfig.WorldSize.X - GameConfig.PlayArea.Size.X) / 2f, (GameConfig.WorldSize.Y - GameConfig.PlayArea.Size.Y) / 2f);
            Rectangle[] borders = Kadro.UI.Border.CreateBorderRectangles(new Rectangle(0, 0, (int)GameConfig.WorldSize.X - 1, (int)GameConfig.WorldSize.Y - 1), thickness, false, true);

            for (int i = 0; i < borders.Length; i++)
            {
                this.AddBorder(this.entityWorld.EntityManager.CreateEntity(), borders[i].Location.ToVector2(), borders[i].Size.ToVector2());
            }
        }
    }
}
