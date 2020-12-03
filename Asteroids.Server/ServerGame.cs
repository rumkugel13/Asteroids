using Asteroids.Shared;
using Kadro.ECS;
using RKDnet;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using Kadro.Extensions;

namespace Asteroids.Server
{
    public class ServerGame : GameRoom
    {
        private readonly SignalManager signalManager;
        private readonly EntityFactory entityFactory;
        private readonly EntityWorld entityWorld;
        private TagCountSystem tagCountSystem;
        private float timeSinceGamestatePacket = 0;
        private readonly float gamestateSendTime = 0.05f;    //50 ms / 20fps
        private int countdownValue;
        private float timeSinceCountdownChange;
        private int playerScenesLoaded = 0;

        public ServerGame()
        {
            this.signalManager = new SignalManager();
            this.entityWorld = new EntityWorld();
            this.entityFactory = new ServerEntityFactory(this.entityWorld, this.Multicast);

            this.entityWorld.SystemManager.AddSystem(new MotionControlSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new LifetimeSystem(this.entityWorld, this.signalManager));
            this.entityWorld.SystemManager.AddSystem(new ShootSystem(this.entityWorld, this.entityFactory));
            this.entityWorld.SystemManager.AddSystem(new MotionSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new RespawnTimeoutSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new CollisionDetectionSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new CollisionResponseSystem(this.entityWorld, this.entityFactory, this.signalManager));
            this.tagCountSystem = new TagCountSystem(this.entityWorld);
            this.entityWorld.SystemManager.AddSystem(this.tagCountSystem);
        }

        protected override void OnIncomingMessage(NetIncomingMessage msg)
        {
            long clientId = msg.SenderConnection.RemoteUniqueIdentifier;
            PacketType packetType = (PacketType)msg.ReadByte();

            switch (packetType)
            {
                case PacketType.Intent: this.HandleIntentPacket(new IntentPacket(msg), clientId); break;
                case PacketType.GameSceneLoaded: this.HandleGameSceneLoadedPacket(new GameSceneLoadedPacket(), clientId); break;
            }
        }

        private void HandleGameSceneLoadedPacket(GameSceneLoadedPacket p, long clientId)
        {
            if (++playerScenesLoaded == this.Players.Count)
            {
                this.StartNewGame();
            }
        }

        protected override void OnStart()
        {
            StartGame();
        }

        private void StartGame()
        {
            this.entityWorld.Initialize();
            this.Multicast.SendReliableMessage(new GameStartPacket());
            this.RoomInfo.GameRoomState = GameRoomStates.InGame;
            this.timeSinceCountdownChange = 0;
            this.countdownValue = GameConfig.Countdown;
        }

        protected override void OnEnd()
        {
            EndGame();
        }

        private void EndGame()
        {
            this.RoomInfo.GameRoomState = GameRoomStates.GameOver;
            this.Multicast.SendReliableMessage(new GameEndPacket());
            this.playerScenesLoaded = 0;
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            this.Update(elapsedSeconds);
        }

        public void Update(float elapsedSeconds)
        {
            if (this.countdownValue <= 0)
            {
                this.entityWorld.Update(elapsedSeconds);

                this.ProcessSignals();

                if (this.tagCountSystem.EntityCount(GameConfig.Asteroid.Tag) == 0)
                {
                    // game won
                    this.EndGame();
                }

                if (this.tagCountSystem.EntityCount(GameConfig.Spaceship.Tag) == 0)
                {
                    // game lost
                    this.EndGame();
                }
            }
            else
            {
                this.entityWorld.Update(0);
                this.timeSinceCountdownChange += elapsedSeconds;
                if (this.timeSinceCountdownChange > 1)
                {
                    this.timeSinceCountdownChange = 0;
                    this.countdownValue--;
                    this.CheckCountdownValue();
                }
            }

            this.timeSinceGamestatePacket += elapsedSeconds;
            this.SendGameState();
        }

        private void CheckCountdownValue()
        {
            if (this.countdownValue > 0)
            {
                this.Multicast.SendReliableMessage(new CountdownPacket() { Value = this.countdownValue });
            }
            else
            {
                // send real gamestartpacket here
                this.Multicast.SendReliableMessage(new CountdownPacket() { Value = this.countdownValue });
            }
        }

        private void ProcessSignals()
        {
            while (this.signalManager.HasSignals())
            {
                Signal signal = this.signalManager.GetNextSignal();
                Entity e = this.entityWorld.EntityManager.GetEntity(signal.EntityId);

                switch (signal.SignalType)
                {
                    case SignalType.LifeLost:
                        this.Multicast.SendReliableMessage(this.CreateScoreboardDataPacket(e, true));
                        break;
                    case SignalType.PointScored:
                        this.Multicast.SendReliableMessage(this.CreateScoreboardDataPacket(e, false));
                        break;
                    case SignalType.EntityDestroyed:
                        this.Multicast.SendReliableMessage(new DestroyEntityPacket() { EntityId = signal.EntityId });
                        break;
                }
            }
        }

        private void HandleIntentPacket(IntentPacket p, long clientId)
        {
            p.EntityId = this.Players[clientId].EntityId;
            this.entityWorld.ComponentManager.GetComponent<IntentComponent>(p.EntityId).IntentManager.SetIntent((Intent)p.Intent, p.IsSet);
            // forward intent to other clients
            this.Multicast.SendReliableMessage(p, clientId);
        }

        private void SendGameState()
        {
            // limit send time to not have to send every frame
            if (this.timeSinceGamestatePacket > this.gamestateSendTime)
            {
                this.Multicast.SendUnreliableMessage(this.CreateGameStatePacket(this.timeSinceGamestatePacket));
                this.timeSinceGamestatePacket = 0;  // NOTE: at fixed timestep of 60hz for main update loop this sends every third update (50ms = 3*16.66ms)
                //this.timeSinceGamestatePacket -= this.gamestateSendTime;
            }
        }

        private EntityListPacket CreateGameStatePacket(float deltaTime)
        {
            EntityListPacket p = new EntityListPacket
            {
                ServerDeltaTime = deltaTime
            };

            var entities = this.entityWorld.EntityManager.GetEntities();
            foreach (Entity e in entities)
            {
                p.Entities.Add(this.CreateEntityStatePacket(e));
            }

            return p;
        }

        private EntityDataPacket CreateEntityStatePacket(Entity e)
        {
            EntityDataPacket p = new EntityDataPacket();

            TransformComponent position = e.GetComponent<TransformComponent>();
            p.EntityId = e.EntityId;
            p.PositionX = position.Position.X;
            p.PositionY = position.Position.Y;
            p.Rotation = position.Rotation;

            MotionComponent motion = e.GetComponent<MotionComponent>();
            if (motion != null)
            {
                p.VelocityX = motion.Velocity.X;
                p.VelocityY = motion.Velocity.Y;
                p.AngularVelocity = motion.AngularVelocity;
            }

            return p;
        }

        private ScoreboardDataPacket CreateScoreboardDataPacket(Entity e, bool respawned)
        {
            ScoreboardDataPacket p = new ScoreboardDataPacket();

            ScoreComponent score = e.GetComponent<ScoreComponent>();
            LifeComponent life = e.GetComponent<LifeComponent>();

            p.EntityId = e.EntityId;
            p.Score = score.Value;
            p.Lives = life.Lives;
            p.EntityRespawned = respawned;

            return p;
        }

        private Vector2[] CreateSpawnPoints(int playerCount)
        {
            Vector2[] spawnPoints = new Vector2[playerCount];
            Vector2 center = GameConfig.PlayArea.Center.ToVector2();
            float radius = Math.Min(GameConfig.PlayArea.Size.X, GameConfig.PlayArea.Size.Y) / 3f;
            float increment = MathHelper.TwoPi / playerCount;
            float theta = (float)new Random().NextDouble() * MathHelper.TwoPi;  //start at random angle

            if (spawnPoints.Length == 1)
            {
                spawnPoints[0] = center;
            }
            else
            {
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    spawnPoints[i] = center + radius * VectorExtensions.AngleToVector2(theta);
                    theta += increment;
                }
            }

            return spawnPoints;
        }

        private void StartNewGame()
        {
            this.Multicast.SendReliableMessage(new CountdownPacket() { Value = this.countdownValue });

            this.entityFactory.CreateBorder();
            this.entityFactory.SpawnAsteroids(GameConfig.PlayArea, GameConfig.AsteroidAmount);
            this.CreateSpaceshipForEveryone();
        }

        private void CreateSpaceshipForEveryone()
        {
            Vector2[] spawnPoints = this.CreateSpawnPoints(this.Players.Count);
            int i = 0;
            foreach (PlayerConnection client in this.Players.Values)
            {
                Entity entity = this.entityWorld.EntityManager.CreateEntity();

                ////this.entityFactory.AddSpaceship(entity, spawnPoints[i], 0, client.IntentManager);
                this.entityFactory.AddSpaceship(entity, spawnPoints[i], 0, new IntentManager());

                client.EntityId = entity.EntityId;
                client.HasControllable = true;
                client.SendReliablePacket(this.CreateSetControllableEntityPacket(entity.EntityId));
                i++;
            }
        }

        private SetControllableEntityPacket CreateSetControllableEntityPacket(uint entityId)
        {
            return new SetControllableEntityPacket
            {
                EntityId = entityId
            };
        }
    }
}
