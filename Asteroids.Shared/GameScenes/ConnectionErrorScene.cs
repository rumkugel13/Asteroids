using Asteroids.Shared;
using Kadro.Input;
using RKDnet;
using Kadro.UI;
using Kadro;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids.Shared
{
    class ConnectionErrorScene : NetworkGameScene
    {
        private TextBlock text;
        private GUIScene scene;
        private Button exit;

        public ConnectionErrorScene(Game game, NetworkManager networkManager, BaseConnection connectionInfo) : base(game, networkManager, connectionInfo)
        {
            this.scene = new GUIScene();

            this.text = new TextBlock(GameConfig.Fonts.Large, "Connection Error");
            this.text.Alignment = Alignment.Center;
            this.text.PreferredPosition = new Point(0, -30);
            this.scene.AddChild(this.text);

            this.exit = new Button(GameConfig.Fonts.Medium);
            this.exit.Alignment = Alignment.Center;
            this.exit.TextBlock.Text = "Exit";
            this.exit.PreferredPosition = new Point(0, 30);
            this.scene.AddChild(this.exit);
        }

        protected override void OnEnter()
        {
            this.Game.IsMouseVisible = true;
            GUISceneManager.SwitchScene(this.scene);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.exit.OnClick())
            {
                this.networkManager.Shutdown();
                SwitchScene<MainMenuScene>();
                return;
            }
        }
    }
}
