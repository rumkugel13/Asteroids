using Asteroids.Shared;
using Kadro.Input;
using RKDnet;
using Kadro.UI;
using Kadro;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace Asteroids.Shared
{
    class WaitingForGameStartScene : NetworkGameScene
    {
        private TextBlock latencyText, roomInfoText, readyText;
        private RoomInfo roomInfo;
        private GUIScene scene;
        private Button backToLobby, btReady;
        private bool isReady;
        private Panel centerPanel;

        public WaitingForGameStartScene(Game game, NetworkManager networkManager, BaseConnection connectionInfo) : base(game, networkManager, connectionInfo)
        {
            this.scene = new GUIScene();

            this.centerPanel = new Panel(new Point(400, 300));
            this.centerPanel.Alignment = Alignment.Center;
            this.scene.AddChild(this.centerPanel);

            this.CreateCenterPanel();
        }

        protected override void OnEnter()
        {
            this.Game.IsMouseVisible = true;
            GUISceneManager.SwitchScene(this.scene);

            this.SetReady(false);
            this.connectionInfo.SendReliablePacket(new RequestRoomInfoPacket());
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
                case PacketType.GameStart: this.HandleGameStartPacket(new GameStartPacket()); break;
                case PacketType.RoomInfo: this.HandleRoomInfoPacket(new RoomInfoPacket(msg)); break;
            }
        }

        private void HandleGameStartPacket(GameStartPacket p)
        {
            SwitchScene<MultiplayerGameScene>();
        }

        private void HandleRoomInfoPacket(RoomInfoPacket p)
        {
            this.roomInfo = p.RoomInfo;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"RoomId: {p.RoomInfo.RoomId}");
            stringBuilder.AppendLine($"Players: {p.RoomInfo.ClientCount}");
            stringBuilder.AppendLine($"Ready: {p.RoomInfo.ReadyCount}");
            this.roomInfoText.Text = stringBuilder.ToString();
        }

        private void SetReady(bool value)
        {
            this.isReady = value;
            this.btReady.TextBlock.Text = value ? "Not Ready" : "Ready";
            this.readyText.Text = value ? "You are ready" : "Enter if ready";
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.backToLobby.OnClick())
            {
                this.SetReady(false);
                this.connectionInfo.SendReliablePacket(new PlayerReadyPacket() { IsReady = this.isReady });

                this.connectionInfo.SendReliablePacket(new LeaveRoomPacket());
                SwitchScene<MultiplayerLobbyScene>();
                return;
            }

            this.networkManager.ReadData(this);

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Enter) || (this.btReady.OnClick() && !this.isReady))
            {
                this.SetReady(true);
                this.connectionInfo.SendReliablePacket(new PlayerReadyPacket() { IsReady = this.isReady });
            }
            else if (this.btReady.OnClick() && this.isReady)
            {
                this.SetReady(false);
                this.connectionInfo.SendReliablePacket(new PlayerReadyPacket() { IsReady = this.isReady });
            }

            this.latencyText.Text = $"Ping: {(this.connectionInfo.Connection.AverageRoundtripTime*1000f):N1}ms";
        }

        private void CreateCenterPanel()
        {
            TextBlock headline = new TextBlock(GameConfig.Fonts.Large, "Preparing Game");
            headline.Alignment = Alignment.Top;
            this.centerPanel.AddChild(headline);

            this.roomInfoText = new TextBlock(GameConfig.Fonts.Large, "No Data");
            this.roomInfoText.Alignment = Alignment.Left;
            this.centerPanel.AddChild(this.roomInfoText);

            this.latencyText = new TextBlock(GameConfig.Fonts.Medium, "Ping:");
            this.latencyText.Alignment = Alignment.BottomLeft;
            this.latencyText.PreferredPosition = new Point(0, -50);
            this.centerPanel.AddChild(this.latencyText);

            this.readyText = new TextBlock(GameConfig.Fonts.Large, "Enter if ready");
            this.readyText.Alignment = Alignment.BottomRight;
            this.readyText.PreferredPosition = new Point(0, -50);
            this.centerPanel.AddChild(this.readyText);

            this.backToLobby = new Button(GameConfig.Fonts.Medium);
            this.backToLobby.Alignment = Alignment.BottomLeft;
            this.backToLobby.TextBlock.Text = "Back to Lobby";
            this.centerPanel.AddChild(this.backToLobby);

            this.btReady = new Button(GameConfig.Fonts.Medium);
            this.btReady.Alignment = Alignment.BottomRight;
            this.btReady.TextBlock.Text = "Ready";
            this.centerPanel.AddChild(this.btReady);
        }
    }
}
