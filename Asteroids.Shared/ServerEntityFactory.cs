using Kadro.ECS;
using RKDnet;
using Microsoft.Xna.Framework;

namespace Asteroids.Shared
{
    public class ServerEntityFactory : EntityFactory
    {
        private Multicast<PlayerConnection> multicast;

        public ServerEntityFactory(EntityWorld entityWorld, Multicast<PlayerConnection> roomDataSender) : base(entityWorld)
        {
            this.multicast = roomDataSender;
        }

        public override void AddAsteroid(Entity e, Vector2 position, float rotation, Vector2 size)
        {
            base.AddAsteroid(e, position, rotation, size);
            this.SendCreateEntity(e.EntityId, (byte)EntityType.Asteroid, position, rotation, size);
        }

        public override void AddProjectile(Entity e, Vector2 position, float rotation)
        {
            base.AddProjectile(e, position, rotation);
            this.SendCreateEntity(e.EntityId, (byte)EntityType.Projectile, position, rotation, Vector2.Zero);
        }

        public override void AddSpaceship(Entity e, Vector2 position, float rotation, IntentManager intentManager)
        {
            base.AddSpaceship(e, position, rotation, intentManager);
            this.SendCreateEntity(e.EntityId, (byte)EntityType.Spaceship, position, rotation, Vector2.Zero);
        }

        public override void AddBorder(Entity e, Vector2 position, Vector2 size)
        {
            base.AddBorder(e, position, size);
            this.SendCreateEntity(e.EntityId, (byte)EntityType.Border, position, 0, size);
        }

        private void SendCreateEntity(uint entityId, byte entityType, Vector2 position, float rotation, Vector2 size)
        {
            CreateEntityPacket p = new CreateEntityPacket();
            p.EntityId = entityId;
            p.EntityType = entityType;
            p.X = position.X;
            p.Y = position.Y;
            p.Rotation = rotation;
            p.SizeX = size.X;
            p.SizeY = size.Y;
            this.multicast.SendReliableMessage(p);    // send new entity to all clients
        }
    }
}
