﻿using Asteroids.Shared;
using Kadro.ECS;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Asteroids
{
    public class ClientEntityFactory : EntityFactory
    {
        public ClientEntityFactory(EntityWorld entityWorld) : base(entityWorld)
        {
        }

        public override void AddAsteroid(Entity e, Vector2 position, float rotation, Vector2 scale)
        {
            base.AddAsteroid(e, position, rotation, scale);
            e.AddComponent(new TextureComponent(Assets.Get<Texture2D>(GameConfig.Folders.Textures,
                (Math.Floor(position.X) % 2 == 0 || Math.Floor(position.Y) % 2 == 0) ? GameConfig.Asteroid.Textures[0] : GameConfig.Asteroid.Textures[1]), scale * GameConfig.Asteroid.Large, 0.4f));
        }

        public override void AddProjectile(Entity e, Vector2 position, float rotation)
        {
            base.AddProjectile(e, position, rotation);
            e.AddComponent(new TextureComponent(Assets.Get<Texture2D>(GameConfig.Folders.Textures, GameConfig.Projectile.DefaultTexture), Vector2.One * GameConfig.Projectile.Size, 0.5f));
        }

        public override void AddMissile(Entity e, Vector2 position, float rotation)
        {
            base.AddMissile(e, position, rotation);
            e.AddComponent(new TextureComponent(Assets.Get<Texture2D>(GameConfig.Folders.Textures, "missile-512"), Vector2.One * GameConfig.Projectile.Size * 4, 0.5f));
        }

        public override void AddSpaceship(Entity e, Vector2 position, float rotation, IntentManager intentManager)
        {
            base.AddSpaceship(e, position, rotation, intentManager);

            e.AddComponent(new TextureComponent(Assets.Get<Texture2D>(GameConfig.Folders.Textures, GameConfig.Spaceship.DefaultTexture), Vector2.One * GameConfig.Spaceship.Size, GameConfig.Spaceship.Origin, 0.7f));
            e.AddComponent(new ParticleSpawnComponent(5, new Vector2(0, GameConfig.Spaceship.Size / 2f), Color.White, MathHelper.PiOver4));

            SpriteAnimation animation = Assets.LoadSpriteSheet(GameConfig.Spaceship.FlameAnimationSheet).GetSpriteAnimation(GameConfig.Spaceship.FlameAnimation);
            animation.Duration = 0.5f;
            animation.Origin = new Vector2(450, 400);
            animation.Initialize();
            e.AddComponent(new SpriteAnimationComponent(animation, 0.02f, new Vector2(0, GameConfig.Spaceship.Size / 2f)));
        }
    }
}
