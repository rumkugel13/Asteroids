using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Kadro;
using Kadro.UI;
using Kadro.Input;
using System.Reflection;
using RKDnet;

namespace Asteroids.Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public partial class AsteroidsGame : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;

        private GUISceneManager sceneManager;
        private NetworkManager networkManager;
        private BaseConnection connectionInfo;

        private PerformanceMetrics performanceCounter;
        private bool showPerformanceStats;
        private Label perfLabel;
        private bool isBorderless;
        private Listbox debugListbox;

        private Label mousePos;

        public AsteroidsGame()
        {
            Logger.Start();
            this.graphics = new GraphicsDeviceManager(this);

            this.Content.RootDirectory = "Content";
            this.InactiveSleepTime = new TimeSpan(0,0,0,0,10);
            //this.TargetElapsedTime = TimeSpan.FromTicks(40000);    //note: only works with fixed step | 166667 = 16.6ms
            //this.graphics.SynchronizeWithVerticalRetrace = true;                //note: caps to display rate, only on fixed step
            //this.IsFixedTimeStep = true;
            //this.MaxElapsedTime = TimeSpan.FromSeconds(1d / 30d);
        }

        private string GetVersionString()
        {
            // note: shared doesnt have version numbers, especially not auto generated ones
            var version = Assembly.GetExecutingAssembly().GetName().Version;    //executing: library where class is; entry: the exe (where main is)

            var buildDate = new DateTime(2000, 1, 1)
               .AddDays(version.Build).AddSeconds(version.Revision * 2);

            var displayableVersion = $"{version} ({buildDate})";

            Console.WriteLine("Current version (inc. build date) = " + displayableVersion);
            return displayableVersion;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if DEBUG
            this.Window.Title = "Asteroids-dev-debug";
#else
            this.Window.Title = "Asteroids-dev-release";
#endif
            this.Window.Title += " - Version: " + this.GetVersionString();
            System.Diagnostics.Trace.WriteLine(DateTime.Now + " " + this.Window.Title);

            UserConfig.Instance = UserConfig.Load();

            WindowSettings.Initialize(this, this.graphics);
            WindowSettings.SetWindowResolution(new Point(UserConfig.Instance.ScreenWidth, UserConfig.Instance.ScreenHeight));
            WindowSettings.SetBorderless(UserConfig.Instance.Borderless);
            WindowSettings.MinWindowResolution = GameConfig.MinWindowSize;
            WindowSettings.UnitsVisible = new Vector2(1280, 720);

            this.renderTarget = new RenderTarget2D(this.GraphicsDevice, WindowSettings.RenderArea.Width, WindowSettings.RenderArea.Height);

            this.sceneManager = new GUISceneManager(this);

            this.Components.Add(new Kadro.Input.KeyboardInput(this));
            this.Components.Add(new MouseInput(this));
            this.Components.Add(new GamepadInput(this));
            this.Components.Add(new TouchpanelInput(this));
            this.performanceCounter = new PerformanceMetrics();

            this.networkManager = new NetworkManager(UserConfig.Instance.NetworkName);
            this.connectionInfo = new BaseConnection();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);
            this.Services.AddService(typeof(SpriteBatch), this.spriteBatch);

            GameConfig.Fonts.VerySmall = this.Content.Load<SpriteFont>("Fonts/Arial_8");
            GameConfig.Fonts.Small = this.Content.Load<SpriteFont>("Fonts/Arial_12");
            GameConfig.Fonts.Medium = this.Content.Load<SpriteFont>("Fonts/Arial_16");
            GameConfig.Fonts.Large = this.Content.Load<SpriteFont>("Fonts/Arial_24");
            GameConfig.Fonts.VeryLarge = this.Content.Load<SpriteFont>("Fonts/Arial_32");

            this.perfLabel = new Label(GameConfig.Fonts.Medium, "Perf");
            this.perfLabel.Alignment = Alignment.Top;
#if DEBUG
            this.mousePos = new Label(GameConfig.Fonts.Small, "MousePos");
            this.mousePos.Alignment = Alignment.Left;
            this.mousePos.SetVisible(false);

            this.CreateDebugPanel();
#endif

            GameScene.AddScene(new MainMenuScene(this));
            GameScene.AddScene(new SingleplayerGameScene(this));
            GameScene.AddScene(new SettingsMenuScene(this));
            GameScene.AddScene(new GameOverScene(this));

            GameScene.AddScene(new ConnectingScene(this, this.networkManager, this.connectionInfo));
            GameScene.AddScene(new ConnectionErrorScene(this, this.networkManager, this.connectionInfo));
            GameScene.AddScene(new MultiplayerGameOverScene(this, this.networkManager, this.connectionInfo));
            GameScene.AddScene(new MultiplayerGameScene(this, this.networkManager, this.connectionInfo));
            GameScene.AddScene(new MultiplayerLobbyScene(this, this.networkManager, this.connectionInfo));
            GameScene.AddScene(new WaitingForGameStartScene(this, this.networkManager, this.connectionInfo));

            GameScene.SwitchScene<MainMenuScene>();

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ////if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            ////    Exit();

            this.performanceCounter.BeginUpdate();

            base.Update(gameTime);

            this.performanceCounter.Update(gameTime);

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.F12))
            {
                WindowSettings.SetBorderless(this.isBorderless = !this.isBorderless);
            }

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Up) || Kadro.Input.KeyboardInput.OnKeyUp(Keys.Left))
            {
                this.sceneManager.TabPrevious();
            }

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Down) || Kadro.Input.KeyboardInput.OnKeyUp(Keys.Right))
            {
                this.sceneManager.TabNext();
            }

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.F3))
            {
                this.showPerformanceStats = !this.showPerformanceStats;
            }

            if (!(this.renderTarget.Width == WindowSettings.RenderArea.Width && this.renderTarget.Height == WindowSettings.RenderArea.Height))
            {
                this.renderTarget.Dispose();
                this.renderTarget = new RenderTarget2D(this.GraphicsDevice, WindowSettings.RenderArea.Width, WindowSettings.RenderArea.Height);
            }

            this.sceneManager.Update(gameTime);
            this.perfLabel.Update();
#if DEBUG
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.F1))
            {
                if(this.debugListbox.IsVisible)
                {
                    this.debugListbox.Hide();
                }
                else
                {
                    this.debugListbox.Show();
                }
            }

            this.debugListbox.Update();

            Point cursor = MouseInput.GetCursorPosition();
            Vector2 viewPos = WindowSettings.ScreenToView(cursor.ToVector2());
            Vector2 worldPos = GameScene.ActiveScene.ViewToWorld(viewPos);
            this.mousePos.TextBlock.Text = $"MousePos: {cursor}\nViewPos: {viewPos}\nWorldPos: {worldPos}";

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.F5))
            {
                if (this.mousePos.IsVisible)
                {
                    this.mousePos.SetVisible(false);
                }
                else
                {
                    this.mousePos.SetVisible(true);
                }
            }

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.L) && this.networkManager.IsActive)
            {
                this.networkManager.SetDebugLatency(0.05f, 0.02f, 0.0f);  // 50ms, 20ms
                this.networkManager.EnableDebugLatency(!this.networkManager.DebugLatency);
            }
#endif
            GameScene.ActiveScene.Update(gameTime);

            this.performanceCounter.EndUpdate();

            if (this.showPerformanceStats)
            {
                this.ShowPerformanceOnWindowTitle();
            }
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);

            // Stop the threads
            // should stop networkmanager if active in multiplayerstate
            if (this.networkManager.IsActive)
            {
                this.networkManager.Shutdown();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.performanceCounter.BeginDraw();
            this.performanceCounter.Draw(gameTime);

            this.spriteBatch.GraphicsDevice.SetRenderTarget(this.renderTarget);
            this.spriteBatch.GraphicsDevice.Clear(Color.Black);

            GameScene.ActiveScene.Draw(gameTime);

            this.spriteBatch.GraphicsDevice.SetRenderTarget(null);
            this.spriteBatch.GraphicsDevice.Clear(Color.Black);

            this.spriteBatch.Begin();

            //note: draw ui stuff and other unrelated things here

            base.Draw(gameTime);
            this.spriteBatch.Draw(this.renderTarget, WindowSettings.RenderArea, Color.White);

            if (this.showPerformanceStats)
            {
                this.perfLabel.Draw(this.spriteBatch);
            }

#if DEBUG
            //this.spriteBatch.DrawString(this.font.SpriteFont, "the quick brown fox jumps over the lazy dog", new Vector2(0, 50), Color.Cyan, 0, Vector2.Zero, this.font.Scale, SpriteEffects.None, 1);
            this.debugListbox.Draw(spriteBatch);
            this.mousePos.Draw(spriteBatch);
#endif
            this.spriteBatch.End();

            this.sceneManager.Draw(gameTime);

            this.performanceCounter.EndDraw();
        }

        private void ShowPerformanceOnWindowTitle()
        {
            //StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append(this.oldTitle);
            //stringBuilder.Append(this.performanceCounter.ToString());
            //this.Window.Title = stringBuilder.ToString();
            this.perfLabel.TextBlock.Text = "Perf: " + this.performanceCounter.ToString();
        }

        private void CreateDebugPanel()
        {
            this.debugListbox = new Listbox(GameConfig.Fonts.Medium);
            this.debugListbox.PreferredSize = new Point(500, 20);
            this.debugListbox.IsDraggable = true;
            this.debugListbox.AddElement("F1 for this list (global)");
            this.debugListbox.AddElement("F2 collision bounds (singleplayergame)");
            this.debugListbox.AddElement("F3 show performance stats (global)");
            this.debugListbox.AddElement("L toggle debug latency (global)");
            this.debugListbox.AddElement("F5 for mouse position (global)");
            this.debugListbox.AddElement("F8 GUI debug mode (global)");
            this.debugListbox.AddElement("F12 for borderless fullscreen (global)");
            this.debugListbox.Hide();
        }
    }
}
