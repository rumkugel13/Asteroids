using Kadro.ECS;
using Kadro.Input;
using Kadro.Particles;
using Kadro.UI;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Kadro.Extensions;

namespace Asteroids.Shared
{
    public class SingleplayerGameScene : GameScene
    {
        private readonly InputManager inputManager;
        private readonly EntityFactory entityFactory;
        private readonly EntityWorld entityWorld;
        private readonly SignalManager signalManager;
        private readonly IntentManager intentManager;
        private readonly Scoreboard scoreboardUI;
        private readonly GUIScene scene;
        private readonly ParticleEngine particleEngine;
        private readonly Background background;
        private Vector2 spaceShipSpawnPosition;
        private TagCountSystem tagCountSystem;
        private Entity myEntity;

        private Label countdown;
        private int countdownValue;
        private float timeSinceCountdownChange;
        private bool isPaused;
        private Button btContinue, btEndGame, btBackToMain;
        private Panel pausePanel;

        public SingleplayerGameScene(Game game) : base(game)
        {
            this.Camera.SetBounds(Vector2.Zero, (GameConfig.WorldSize - WindowSettings.RenderArea.Size + WindowSettings.RenderArea.Location).ToVector2());
            this.background = new Background(Assets.Get<Texture2D>(GameConfig.Folders.Particles, "circle"), new Rectangle(Point.Zero, GameConfig.WorldSize));

            this.signalManager = new SignalManager();
            this.intentManager = new IntentManager();
            this.inputManager = new InputManager(this.intentManager);
            this.scene = new GUIScene();

            List<Texture2D> list = new List<Texture2D>
            {
                Assets.Get<Texture2D>(GameConfig.Folders.Particles,"circle")
            };
            this.particleEngine = new ParticleEngine(list, Vector2.Zero, 0, 50, false);

            this.scoreboardUI = new Scoreboard(GameConfig.Fonts.Medium, false);
            this.scoreboardUI.Hide();
            this.scene.AddChild(scoreboardUI);

            this.spaceShipSpawnPosition = GameConfig.PlayArea.Center.ToVector2();
            this.entityWorld = new EntityWorld();
            this.entityFactory = new ClientEntityFactory(this.entityWorld);

            this.entityWorld.SystemManager.AddSystem(new MotionControlSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new LifetimeSystem(this.entityWorld, this.signalManager));
            this.entityWorld.SystemManager.AddSystem(new ShootSystem(this.entityWorld, this.entityFactory));
            this.entityWorld.SystemManager.AddSystem(new MotionSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new CollisionDetectionSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new CollisionResponseSystem(this.entityWorld, this.entityFactory, this.signalManager));
            this.entityWorld.SystemManager.AddSystem(new RespawnTimeoutSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new TargetingSystem(this.entityWorld));
            this.entityWorld.SystemManager.AddSystem(new TextureRenderSystem(this.entityWorld));
            //this.entityWorld.SystemManager.AddSystem(new ParticleSystem(this.entityWorld, this.particleEngine));
            this.entityWorld.SystemManager.AddSystem(new ScoreboardSystem(this.entityWorld, scoreboardUI));
            this.entityWorld.SystemManager.AddSystem(new SpriteAnimationRenderSystem(this.entityWorld));
            this.tagCountSystem = (TagCountSystem)this.entityWorld.SystemManager.AddSystem(new TagCountSystem(this.entityWorld));

            this.CreateScene();

            this.CreatePauseMenu();
        }

        protected override void OnDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.background.Draw(spriteBatch);
            this.entityWorld.Draw(elapsedSeconds, spriteBatch);
            this.particleEngine.Draw(spriteBatch);

            //if (MouseManager.IsLeftButtonDown())
            //{
            //    spriteBatch.DrawCircle(new Circle(this.entityCamera.ScreenToWorld(MouseManager.GetMouseCursorPosition() * GameConfig.ScreenToWorldScale), 100), Color.Pink, 3);
            //}

            if (Kadro.Input.KeyboardInput.IsKeyDown(Keys.B))
            {
                Vector2 position = this.Camera.ViewToWorld(WindowSettings.ScreenToView(MouseInput.GetCursorPosition().ToVector2()));
                spriteBatch.DrawCircle(position, 2, Color.Red, 1);
            }
        }

        protected override void OnEnter()
        {
            this.Game.IsMouseVisible = false;
            GUISceneManager.SwitchScene(this.scene);

            this.isPaused = false;
            this.pausePanel.Hide();
            this.Initialize();
        }

        private void Initialize()
        {
            this.entityWorld.Initialize();
            this.intentManager.Initialize();
            this.scoreboardUI.Initialize();
            this.background.Create();
            this.StartNewGame();
        }

        protected override void OnExit()
        {
            
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.btContinue.OnClick())
            {
                if (!isPaused)
                {
                    this.Game.IsMouseVisible = true;
                    this.isPaused = true;
                    this.pausePanel.Show();
                }
                else
                {
                    this.isPaused = false;
                    this.pausePanel.Hide();
                    this.Game.IsMouseVisible = false;
                }
            }

            if (isPaused)
            {
                if (this.btEndGame.OnClick())
                {
                    SwitchScene<GameOverScene>();
                    ////this.pausePanel.Hide();
                }

                if (this.btBackToMain.OnClick())
                {
                    SwitchScene<MainMenuScene>();
                }

                return;
            }

            ////if (KeyboardManager.IsKeyDown(Keys.Tab))    //only show scoreboard in multiplayer
            ////{
            ////    this.scoreboardUI.Show();
            ////}
            ////else
            ////{
            ////    this.scoreboardUI.Hide();
            ////}

            this.Camera.Origin = WindowSettings.RenderArea.Size.ToVector2() / 2f;
            this.Camera.SetBounds(Vector2.Zero, (GameConfig.WorldSize - WindowSettings.RenderArea.Size + WindowSettings.RenderArea.Location).ToVector2());

#if DEBUG
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.N))
            {
                this.entityFactory.CreateAsteroid(GameConfig.PlayArea);
            }

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.M))
            {
                this.entityFactory.SpawnAsteroids(GameConfig.PlayArea, 100);
            }

            if (MouseInput.OnScrollWheelUp())
            {
                this.Camera.Zoom *= 1.125f;
                Console.WriteLine($"EntityZoom: {this.Camera.Zoom}");
            }

            if (MouseInput.OnScrollWheelDown())
            {
                this.Camera.Zoom /= 1.125f;
                Console.WriteLine($"EntityZoom: {this.Camera.Zoom}");
            }
#endif

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            this.inputManager.Update();

            if (this.countdownValue <= 0)
            {
                this.entityWorld.Update(elapsedSeconds);
                this.particleEngine.Update(elapsedSeconds);

                this.ProcessSignals();

                if (this.tagCountSystem.EntityCount(GameConfig.Asteroid.Tag) == 0)
                {
                    // game won
                    SwitchScene<GameOverScene>();
                }

                if (this.tagCountSystem.EntityCount(GameConfig.Spaceship.Tag) == 0)
                {
                    // game lost
                    SwitchScene<GameOverScene>();
                }

                if (myEntity != null)
                {

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

            if (this.myEntity != null)
            {
                TransformComponent transform = this.entityWorld.ComponentManager.GetComponent<TransformComponent>(this.myEntity.EntityId);
                this.Camera.FocusOn(transform.Position * WindowSettings.UnitScale, 0.05f);
            }
        }

        private void ProcessSignals()
        {
            while (this.signalManager.HasSignals())
            {
                Signal signal = this.signalManager.GetNextSignal();

                switch (signal.SignalType)
                {
                    case SignalType.LifeLost:
                        break;
                    case SignalType.PointScored:
                        break;
                    case SignalType.EntityCreated:
                        break;
                    case SignalType.EntityDestroyed:
                        break;
                }
            }
        }

        private void CheckCountdownValue()
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

        private void StartNewGame()
        {
            this.timeSinceCountdownChange = 0;
            this.countdownValue = GameConfig.Countdown;
            this.countdown.TextBlock.Text = "Countdown: " + this.countdownValue;
            this.countdown.SetVisible(true);

            this.entityFactory.CreateBorder();
            this.entityFactory.SpawnAsteroids(GameConfig.PlayArea, GameConfig.AsteroidAmount);
            this.CreateSpaceship();
        }

        private void CreateSpaceship()
        {
            Entity e = this.entityWorld.EntityManager.CreateEntity();
            this.entityFactory.AddSpaceship(e, this.spaceShipSpawnPosition, 0, this.intentManager);
            this.SetControllableEntity(e.EntityId);
        }

        private void SetControllableEntity(uint entityId)
        {
            this.scoreboardUI.SetMyEntity(entityId);
            this.myEntity = this.entityWorld.EntityManager.GetEntity(entityId);
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

        private void CreatePauseMenu()
        {
            this.pausePanel = new Panel(this.scene.PreferredSize);
            this.pausePanel.Alignment = Alignment.Stretch;
            this.pausePanel.Opacity = 0.75f;
            this.scene.AddChild(this.pausePanel);

            TextBlock headLine = new TextBlock(GameConfig.Fonts.Large, "Pause Menu");
            headLine.Alignment = Alignment.Center;
            headLine.PreferredPosition = new Point(0, -150);
            this.pausePanel.AddChild(headLine);

            this.btContinue = new Button(GameConfig.Fonts.Medium, "Continue");
            this.btContinue.Alignment = Alignment.Center;
            this.btContinue.PreferredPosition = new Point(0, -50);
            this.pausePanel.AddChild(this.btContinue);

            this.btEndGame = new Button(GameConfig.Fonts.Medium, "End Game");
            this.btEndGame.Alignment = Alignment.Center;
            this.pausePanel.AddChild(this.btEndGame);

            this.btBackToMain = new Button(GameConfig.Fonts.Medium, "Back to Main");
            this.btBackToMain.Alignment = Alignment.Center;
            this.btBackToMain.PreferredPosition = new Point(0, 50);
            this.pausePanel.AddChild(this.btBackToMain);

            this.pausePanel.Hide();
        }
    }
}