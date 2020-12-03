using Kadro.Input;
using RKDnet;
using Kadro.UI;
using Kadro;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Asteroids.Shared
{
    class ConnectingScene : NetworkGameScene
    {
        private TextBlock text;
        private GUIScene scene;
        private Button exit;

        public ConnectingScene(Game game, NetworkManager networkManager, BaseConnection connectionInfo) : base(game, networkManager, connectionInfo)
        {
            this.scene = new GUIScene();

            this.text = new TextBlock(GameConfig.Fonts.Large, "Connecting to server...");
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

            this.networkManager.Start();
            this.TryConnect();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.exit.OnClick())
            {
                this.networkManager.Shutdown();
                SwitchScene<MainMenuScene>();
                return;
            }

            this.networkManager.ReadData(this);
        }

        public override void OnConnected(BaseConnection connectionInfo)
        {
            this.connectionInfo.Connection = connectionInfo.Connection;
            SwitchScene<MultiplayerLobbyScene>();
        }

        public override void OnDisconnected(long clientId, string reason)
        {
            SwitchScene<ConnectionErrorScene>();
            System.Console.WriteLine("Client: " + reason);
        }

        private void TryConnect()
        {
#if DEBUG
            this.networkManager.Connect(UserConfig.Instance.DebugServer, UserConfig.Instance.DebugPort);
#else
            this.networkManager.Connect(UserConfig.Instance.DefaultServer, UserConfig.Instance.DefaultPort);
#endif
        }
    }
}
