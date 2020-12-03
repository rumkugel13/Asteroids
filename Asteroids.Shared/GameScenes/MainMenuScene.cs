using System;
using Asteroids.Shared;
using Kadro.Input;
using Kadro.UI;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids.Shared
{
    public class MainMenuScene : GameScene
    {
        private GUIScene scene;
        private Button singleplayer, multiplayer, settings, exit;

        public MainMenuScene(Game game) : base(game)
        {
            this.scene = new GUIScene();

            this.CreateMainMenu();
        }

        //private enum MainMenuItems
        //{
        //    Singleplayer, Multiplayer, Settings, Exit,
        //}

        protected override void OnEnter()
        {
            //Microsoft.Xna.Framework.Input.Mouse.SetCursor(Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(new Texture2D(game.GraphicsDevice,1,1),1,1));
            this.Game.IsMouseVisible = true;
            GUISceneManager.SwitchScene(this.scene);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.exit.OnClick())
            {
                this.Game.Exit();
                return;
            }

            if (this.singleplayer.OnClick())
            {
                SwitchScene<SingleplayerGameScene>();
            }

            if (this.multiplayer.OnClick())
            {
                SwitchScene<ConnectingScene>();
            }

            if (this.settings.OnClick())
            {
                SwitchScene<SettingsMenuScene>();
            }
        }

        private void CreateMainMenu()
        {
            TextBlock headLine = new TextBlock(GameConfig.Fonts.Large, "Main Menu");
            headLine.Alignment = Alignment.Center;
            headLine.PreferredPosition = new Point(0, -170);
            this.scene.AddChild(headLine);

            this.singleplayer = new Button(GameConfig.Fonts.Large, "Singleplayer");
            this.singleplayer.Alignment = Alignment.Center;
            this.singleplayer.PreferredSize = new Point(400, 50);
            this.singleplayer.PreferredPosition = new Point(0, -90);
            this.singleplayer.Border.Thickness = 4;
            this.scene.AddChild(this.singleplayer);

            this.multiplayer = new Button(GameConfig.Fonts.Large, "Multiplayer");
            this.multiplayer.Alignment = Alignment.Center;
            this.multiplayer.PreferredSize = new Point(400, 50);
            this.multiplayer.PreferredPosition = new Point(0, -30);
            this.multiplayer.Border.Thickness = 4;
            this.scene.AddChild(this.multiplayer);

            this.settings = new Button(GameConfig.Fonts.Large, "Settings");
            this.settings.Alignment = Alignment.Center;
            this.settings.PreferredSize = new Point(400, 50);
            this.settings.PreferredPosition = new Point(0, 30);
            this.settings.Border.Thickness = 4;
            this.scene.AddChild(this.settings);

            this.exit = new Button(GameConfig.Fonts.Large, "Exit");
            this.exit.Alignment = Alignment.Center;
            this.exit.PreferredSize = new Point(400, 50);
            this.exit.PreferredPosition = new Point(0, 90);
            this.exit.Border.Thickness = 4;
            this.scene.AddChild(this.exit);
        }
    }
}
