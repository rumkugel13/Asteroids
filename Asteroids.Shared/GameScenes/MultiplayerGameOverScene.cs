using Asteroids.Shared;
using Kadro.Input;
using Kadro.UI;
using Kadro;
using RKDnet;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids.Shared
{
    class MultiplayerGameOverScene : NetworkGameScene
    {
        private TextBlock text;
        private GUIScene scene;
        private Button backToLobby;

        public MultiplayerGameOverScene(Game game, NetworkManager networkManager, BaseConnection connectionInfo) : base(game, networkManager, connectionInfo)
        {
            this.scene = new GUIScene();

            this.text = new TextBlock(GameConfig.Fonts.Large, "Game Over!");
            this.text.Alignment = Alignment.Center;
            this.text.PreferredPosition = new Point(0, -30);
            this.scene.AddChild(this.text);

            this.backToLobby = new Button(GameConfig.Fonts.Medium);
            this.backToLobby.Alignment = Alignment.Center;
            this.backToLobby.TextBlock.Text = "Back to Lobby";
            this.backToLobby.PreferredPosition = new Point(0, 30);
            this.scene.AddChild(this.backToLobby);
        }

        protected override void OnEnter()
        {
            this.Game.IsMouseVisible = true;
            GUISceneManager.SwitchScene(this.scene);
        }

        public override void OnDisconnected(long clientId, string reason)
        {
            SwitchScene<ConnectionErrorScene>();
            System.Console.WriteLine("Client: " + reason);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.backToLobby.OnClick())
            {
                SwitchScene<MultiplayerLobbyScene>();
                return;
            }

            this.networkManager.ReadData(this);
        }
    }
}
