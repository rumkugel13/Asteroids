using Kadro.Physics.Colliders;
using Kadro.Physics;
using Kadro.ECS;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Asteroids.Shared
{
    public class CollidableComponent : IComponent
    {
        /// <summary>
        /// The type of the entity
        /// </summary>
        public CollisionLayer CollisionLayer { get; }

        /// <summary>
        /// Which Entities it collides with
        /// </summary>
        public CollisionLayer CollisionMask { get; }

        public ICollider Collider { get; }

        public List<CollidableComponent> Collisions { get; }

        public bool HasCollisions => this.Collisions.Count > 0;

        public uint EntityId { get; }

        public CollidableComponent(long layer, long collisionMask, ICollider collider, uint entityId)
        {
            this.CollisionLayer = new CollisionLayer(layer);
            this.CollisionMask = new CollisionLayer(collisionMask);
            this.Collider = collider;
            this.EntityId = entityId;
            this.Collisions = new List<CollidableComponent>();
        }

        public void DebugDraw(SpriteBatch spriteBatch, int lineWidth)
        {
            if (this.HasCollisions)
            {
                this.Collider.Draw(spriteBatch, Microsoft.Xna.Framework.Color.Green, lineWidth);
            }
            else
            {
                this.Collider.Draw(spriteBatch, Microsoft.Xna.Framework.Color.White, lineWidth);
            }
        }

        public virtual void OnCollisionEnter(CollidableComponent other)
        {

        }

        public virtual void OnCollisionStay(CollidableComponent other)
        {

        }

        public virtual void OnCollisionExit(CollidableComponent other)
        {

        }
    }
}
