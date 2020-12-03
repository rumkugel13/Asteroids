using System;
using System.Text;
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
    class MultiplayerLobbyScene : NetworkGameScene
    {
        private TextBlock lobbyInfo;
        private GUIScene scene;
        private Button btJoin, backToMain;
        private Panel centerPanel;

        public MultiplayerLobbyScene(Game game, NetworkManager networkManager, BaseConnection connectionInfo) : base(game, networkManager, connectionInfo)
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
            this.connectionInfo.SendReliablePacket(new RequestLobbyInfoPacket());
        }

        public override void OnDisconnected(long clientId, string reason)
        {
            SwitchScene<ConnectionErrorScene>();
            System.Console.WriteLine("Client: " + reason);
        }

        public override void ProcessIncomingMessage(NetIncomingMessage msg)
        {
            PacketType type = (PacketType)msg.ReadByte();

            switch(type)
            {
                case PacketType.AcceptJoinRequest: this.HandleAcceptJoinPacket(new AcceptJoinRequestPacket(msg)); break;
                case PacketType.DeclineJoinRequest: this.HandleDeclineJoinPacket(new DeclineJoinRequestPacket(msg)); break;
                case PacketType.LobbyInfo: this.HandleLobbyInfoPacket(new LobbyInfoPacket(msg)); break;
            }
        }

        private void HandleLobbyInfoPacket(LobbyInfoPacket p)
        {
            // show on screen
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Players Total: {p.ClientCount}");
            stringBuilder.AppendLine($"Players InGame: {p.InGameCount}");
            stringBuilder.AppendLine($"Rooms Total: {p.RoomCount}");

            this.lobbyInfo.Text = stringBuilder.ToString();
        }

        private void HandleAcceptJoinPacket(AcceptJoinRequestPacket p)
        {
            SwitchScene<WaitingForGameStartScene>();
        }

        private void HandleDeclineJoinPacket(DeclineJoinRequestPacket p)
        {
            // dispay info message
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.backToMain.OnClick())
            {
                this.networkManager.Shutdown();
                SwitchScene<MainMenuScene>();
                return;
            }

            this.networkManager.ReadData(this);

            if (this.btJoin.OnClick())
            {
                this.connectionInfo.SendReliablePacket(new JoinFreeRoomPacket());
            }

            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Enter))
            {
                this.connectionInfo.SendReliablePacket(new JoinFreeRoomPacket());
            }
        }

        private void CreateCenterPanel()
        {
            TextBlock headline = new TextBlock(GameConfig.Fonts.Large, "Server Lobby");
            headline.Alignment = Alignment.Top;
            this.centerPanel.AddChild(headline);

            this.lobbyInfo = new TextBlock(GameConfig.Fonts.Large, "Players Total:\nPlayers InGame:\nRooms Total:");
            this.lobbyInfo.Alignment = Alignment.Left;
            this.centerPanel.AddChild(this.lobbyInfo);

            this.backToMain = new Button(GameConfig.Fonts.Medium);
            this.backToMain.Alignment = Alignment.BottomLeft;
            this.backToMain.TextBlock.Text = "Back to Main";
            this.centerPanel.AddChild(this.backToMain);

            this.btJoin = new Button(GameConfig.Fonts.Medium);
            this.btJoin.Alignment = Alignment.BottomRight;
            this.btJoin.TextBlock.Text = "Join free room";
            this.centerPanel.AddChild(btJoin);
        }
    }
}
