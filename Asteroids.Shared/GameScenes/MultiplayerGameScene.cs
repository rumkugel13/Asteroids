using Asteroids.Shared;
using Kadro.ECS;
using Kadro.Input;
using RKDnet;
using Kadro.Particles;
using Kadro.UI;
using Kadro;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Asteroids.Shared
{
    class MultiplayerGameScene : NetworkGameScene
    {
        private readonly EntityFactory entityFactory;
        private readonly IntentManager intentManager;
        private readonly ClientInputManager clientInputManager;
        private readonly GUIScene scene;
        private readonly EntityWorld entityWorld;
        private readonly Scoreboard scoreboardUI;
        private readonly ParticleEngine particleEngine;
        private Entity myEntity;

        private readonly Background background;
        private int countdownValue;
        private Label countdown;
        private Button btContinue, btEndGame, btBackToLobby;
        private Panel escPanel;

        public MultiplayerGameScene(Game game, NetworkManager networkManager, BaseConnection connectionInfo) : base (game, networkManager, connectionInfo)
        {
            this.scene = new GUIScene();
            this.background = new Background(Assets.Get<Texture2D>(GameConfig.Folders.Particles, "circle"), new Rectangle(Point.Zero, GameConfig.WorldSize));

            this.intentManager = new IntentManager();
            this.clientInputManager = new ClientInputManager(this.connectionInfo, this.intentManager);

            List<Texture2D> list = new List<Texture2D>
            {
                Assets.Get<Texture2D>(GameConfig.Folders.Particles,"circle")
            };
            this.particleEngine = new ParticleEngine(list, Vector2.Zero, 0, 50, false);

            this.entityWorld = new EntityWorld();
            this.entityFactory = new ClientEntityFactory(this.entityWorld);

            this.scoreboardUI = new Scoreboard(GameConfig.Fonts.Medium, true);
            this.scoreboardUI.Hide();
            this.scene.AddChild(this.scoreboardUI);

            this.entityWorld.SystemManager.AddSystem(new MotionSystem(this.entityWorld)); //dead reckoning (for client side prediction)
            this.entityWorld.SystemManager.AddSystem(new RespawnTimeoutSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new TextureRenderSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new ScoreboardSystem(this.entityWorld, this.scoreboardUI));
            this.entityWorld.SystemManager.AddSystem(new ParticleSystem(this.entityWorld, this.particleEngine));

            this.CreateScene();
            
            this.CreateEscMenu();
        }

        protected override void OnDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.background.Draw(spriteBatch);
            this.entityWorld.Draw(elapsedSeconds, spriteBatch);
            this.particleEngine.Draw(spriteBatch);
        }

        protected override void OnEnter()
        {
            this.Game.IsMouseVisible = false;
            GUISceneManager.SwitchScene(this.scene);

            this.connectionInfo.SendReliablePacket(new GameSceneLoadedPacket());
            //gamestartpacket arrives in other class
            this.countdown.SetVisible(true);
            this.Initialize();
        }

        private void Initialize()
        {
            this.entityWorld.Initialize();
            this.intentManager.Initialize();
            this.scoreboardUI.Initialize();
            this.background.Create();
        }

        protected override void OnExit()
        {
            this.escPanel.Hide();
            this.connectionInfo.SendReliablePacket(new LeaveRoomPacket());
            this.myEntity = null;
        }

        public override void OnDisconnected(long clientId, string reason)
        {
            SwitchScene<ConnectionErrorScene>();
            System.Console.WriteLine("Client: " + reason);
        }

        public override void ProcessIncomingMessage(NetIncomingMessage msg)
        {
            PacketType type = (PacketType)msg.ReadByte();
            
            switch (type)
            {
                case PacketType.CreateEntity: this.HandleCreateEntity(new CreateEntityPacket(msg)); break;
                case PacketType.DestroyEntity: this.HandleDestroyEntity(new DestroyEntityPacket(msg)); break;
                case PacketType.EntityData: this.HandleEntityData(new EntityDataPacket(msg)); break;
                case PacketType.EntityList: this.HandleEntityList(new EntityListPacket(msg)); break;
                case PacketType.Position: this.HandlePositionData(new PositionPacket(msg)); break;
                case PacketType.ScoreboardData: this.HandleScoreboardData(new ScoreboardDataPacket(msg)); break;
                case PacketType.SetControllableEntity: this.HandleSetControllablePacket(new SetControllableEntityPacket(msg)); break;
                case PacketType.GameEnd: this.HandleGameEndPacket(new GameEndPacket()); break;
                case PacketType.Intent: this.HandleIntentPacket(new IntentPacket(msg)); break;
                case PacketType.Countdown: this.HandleCountdownPacket(new CountdownPacket(msg)); break;
                case PacketType.ClientPing: this.HandlePlayerPingPacket(new PlayerPingPacket(msg)); break;
            }
        }

        private void HandleCountdownPacket(CountdownPacket p)
        {
            this.countdownValue = p.Value;
            this.UpdateCountdownValue();
        }

        private void UpdateCountdownValue()
        {
            if (this.countdownValue > 0)
            {
                this.countdown.TextBlock.Text = "Countdown: " + this.countdownValue;
            }
            else
            {
                this.countdown.SetVisible(false);
            }
        }

        private void HandleIntentPacket(IntentPacket p)
        {
            this.entityWorld.ComponentManager.GetComponent<IntentComponent>(p.EntityId)?.IntentManager.SetIntent((Intent)p.Intent, p.IsSet);
        }

        private void HandleGameEndPacket(GameEndPacket p)
        {
            SwitchScene<MultiplayerGameOverScene>();
        }

        private void HandleCreateEntity(CreateEntityPacket p)
        {
            switch ((EntityType)p.EntityType)
            {
                case EntityType.Spaceship:
                    this.entityFactory.AddSpaceship(this.entityWorld.EntityManager.CreateEntity(p.EntityId), new Vector2(p.X, p.Y), p.Rotation, new IntentManager());
                    break;
                case EntityType.Projectile:
                    this.entityFactory.AddProjectile(this.entityWorld.EntityManager.CreateEntity(p.EntityId), new Vector2(p.X, p.Y), p.Rotation);
                    break;
                case EntityType.Asteroid:
                    this.entityFactory.AddAsteroid(this.entityWorld.EntityManager.CreateEntity(p.EntityId), new Vector2(p.X, p.Y), p.Rotation, new Vector2(p.SizeX, p.SizeY));
                    break;
                case EntityType.Border:
                    this.entityFactory.AddBorder(this.entityWorld.EntityManager.CreateEntity(p.EntityId), new Vector2(p.X, p.Y), new Vector2(p.SizeX, p.SizeY));
                    break;
            }
        }

        private void HandleDestroyEntity(DestroyEntityPacket p)
        {
            //this.entityFactory.RemoveEntity(p.EntityId);
            this.entityWorld.EntityManager.DestroyEntity(p.EntityId);
        }

        private void HandleEntityData(EntityDataPacket p, float serverDeltaTime = 0)
        {
            Entity e = this.entityWorld.EntityManager.GetEntity(p.EntityId);

            if (e != null)
            {
                TransformComponent transform = e.GetComponent<TransformComponent>();
                System.Diagnostics.Debug.Assert(transform != null, transform.GetType() + " not found");

                // TODO: better client side prediction:
                /*  position + (velocity * network_latency)
                 *  position + (velocity * network_latency) + (acceleration * network_latency^2) / 2
                 *      might be inaccurate, due to high possible latency, > 100ms
                 *  
                 *  latency too high: extrapolate position using above (dead reckoning)?
                 *  latency in acceptable range: lerp predicted position and received position
                 */

                // adjust velocity continuosly, unless difference is too big, then change position
                float deltaValue = 30f;   //pixel (unit; due to new scale) offset
                Vector2 dataPosition = new Vector2(p.PositionX, p.PositionY);

                if (!this.IsInbetween(transform.Position.X, dataPosition.X, deltaValue) || !this.IsInbetween(transform.Position.Y, dataPosition.Y, deltaValue))
                    transform.Position = dataPosition;
                if (!this.IsInbetween(transform.Rotation, p.Rotation, deltaValue / 120f))
                    transform.Rotation = p.Rotation;

                MotionComponent motion = e.GetComponent<MotionComponent>();
                if (motion != null)
                {
                    motion.Velocity = new Vector2(p.VelocityX, p.VelocityY);
                    motion.AngularVelocity = p.AngularVelocity;

                    //serverDeltaTime = serverDeltaTime;  //time in totalseconds
                    float convergeFactor = 0.1f;    //percentage for adding velocity so that mismatched position catches up

                    motion.Velocity += new Vector2((dataPosition.X - transform.Position.X) * convergeFactor * 100, (dataPosition.Y - transform.Position.Y) * convergeFactor * 100);
                    motion.AngularVelocity += ((p.Rotation - transform.Rotation) * convergeFactor * 100);
                }
            }
        }

        private bool IsInbetween(float value, float second, float delta)
        {
            return (value >= second - delta && value <= second + delta);
        }

        private void HandleEntityList(EntityListPacket p)
        {
            foreach (EntityDataPacket e in p.Entities)
            {
                this.HandleEntityData(e, p.ServerDeltaTime);
            }
        }

        private void HandleScoreboardData(ScoreboardDataPacket p)
        {
            Entity e = this.entityWorld.EntityManager.GetEntity(p.EntityId);

            if (e != null)
            {
                ScoreComponent score = e.GetComponent<ScoreComponent>();
                LifeComponent life = e.GetComponent<LifeComponent>();

                System.Diagnostics.Debug.Assert(score != null, "PositionComponent not found");
                System.Diagnostics.Debug.Assert(life != null, "VelocityComponent not found");

                score.Value = p.Score;
                life.Lives = p.Lives;
                if (p.EntityRespawned)
                {
                    e.AddComponent(new ShieldComponent());
                    e.GetComponent<RespawnComponent>().TimeSinceRespawn = 0;
                }
            }
        }

        private void HandlePlayerPingPacket(PlayerPingPacket p)
        {
            Entity e = this.entityWorld.EntityManager.GetEntity(p.EntityId);

            if (e != null)
            {
                ScoreComponent score = e.GetComponent<ScoreComponent>();
                LifeComponent life = e.GetComponent<LifeComponent>();

                System.Diagnostics.Debug.Assert(score != null, "PositionComponent not found");
                System.Diagnostics.Debug.Assert(life != null, "VelocityComponent not found");

                this.scoreboardUI.AddOrUpdate(p.EntityId, p.Ping);
            }
        }

        private void HandlePositionData(PositionPacket p)
        {

        }

        private void HandleSetControllablePacket(SetControllableEntityPacket p)
        {
            this.entityWorld.ComponentManager.GetComponent<IntentComponent>(p.EntityId).IntentManager = this.intentManager;
            this.SetControllableEntity(p.EntityId);
        }

        private void SetControllableEntity(uint entityId)
        {
            this.scoreboardUI.SetMyEntity(entityId);
            this.myEntity = this.entityWorld.EntityManager.GetEntity(entityId);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            this.networkManager.ReadData(this);

            if (!this.escPanel.IsVisible && Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape))
            {
                //this.multiplayerStateManager.PreviousState();
                this.Game.IsMouseVisible = true;
                this.escPanel.Show();
            }
            else if (this.escPanel.IsVisible)
            {
                if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.btContinue.OnClick())
                {
                    this.escPanel.Hide();
                    this.Game.IsMouseVisible = false;
                }

                if (this.btEndGame.OnClick())
                {
                    SwitchScene<MultiplayerGameOverScene>();
                    return;
                }

                if (this.btBackToLobby.OnClick())
                {
                    SwitchScene<MultiplayerLobbyScene>();
                    return;
                }
            }

            if (!this.escPanel.IsVisible && Kadro.Input.KeyboardInput.IsKeyDown(Keys.Tab))
            {
                this.scoreboardUI.Show();
            }
            else
            {
                this.scoreboardUI.Hide();
            }

            this.Camera.Origin = WindowSettings.RenderArea.Size.ToVector2() / 2f;
            this.Camera.SetBounds(Vector2.Zero, (GameConfig.WorldSize - WindowSettings.RenderArea.Size + WindowSettings.RenderArea.Location).ToVector2());

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (this.countdownValue <= 0)
            {
                this.clientInputManager.Update();
                this.entityWorld.Update(elapsedSeconds);
                this.particleEngine.Update(elapsedSeconds);
            }
            else
            {
                this.entityWorld.Update(0);
            }

            if (this.myEntity != null)
            {
                TransformComponent transform = this.entityWorld.ComponentManager.GetComponent<TransformComponent>(this.myEntity.EntityId);
                this.Camera.FocusOn(transform.Position * WindowSettings.UnitScale, 0.05f);
            }
        }

        private void CreateScene()
        {
            this.countdown = new Label(GameConfig.Fonts.Large, "Countdown: ");
            this.countdown.Alignment = Alignment.Center;
            this.countdown.Border.Thickness = 0;
            this.countdown.Opacity = 0.75f;
            this.countdown.SetVisible(false);
            this.scene.AddChild(this.countdown);
        }

        private void CreateEscMenu()
        {
            this.escPanel = new Panel(this.scene.PreferredSize);
            this.escPanel.Alignment = Alignment.Stretch;
            this.escPanel.Opacity = 0.75f;
            this.scene.AddChild(this.escPanel);

            TextBlock headLine = new TextBlock(GameConfig.Fonts.Large, "Escape Menu");
            headLine.Alignment = Alignment.Center;
            headLine.PreferredPosition = new Point(0, -150);
            this.escPanel.AddChild(headLine);

            this.btContinue = new Button(GameConfig.Fonts.Medium, "Continue");
            this.btContinue.Alignment = Alignment.Center;
            this.btContinue.PreferredPosition = new Point(0, -50);
            this.escPanel.AddChild(this.btContinue);

            this.btEndGame = new Button(GameConfig.Fonts.Medium, "End Game");
            this.btEndGame.Alignment = Alignment.Center;
            this.escPanel.AddChild(this.btEndGame);

            this.btBackToLobby = new Button(GameConfig.Fonts.Medium, "Back to Lobby");
            this.btBackToLobby.Alignment = Alignment.Center;
            this.btBackToLobby.PreferredPosition = new Point(0, 50);
            this.escPanel.AddChild(this.btBackToLobby);

            this.escPanel.Hide();
        }
    }
}
