using RKDnet;
using Lidgren.Network;
using Kadro;
using Microsoft.Xna.Framework;

namespace Asteroids.Shared
{
    public abstract class NetworkGameScene : GameScene, INetworkHandler
    {
        protected NetworkManager networkManager;
        protected BaseConnection connectionInfo;

        public NetworkGameScene(Game game, NetworkManager networkManager, BaseConnection connectionInfo) : base(game)
        {
            this.networkManager = networkManager;
            this.connectionInfo = connectionInfo;
        }

        public virtual void OnConnected(BaseConnection connectionInfo)
        {

        }

        public virtual void OnDisconnected(long clientId, string reason)
        {
            
        }

        public virtual void OnLatencyUpdated(long clientId, float RTT)
        {

        }

        public virtual void ProcessIncomingMessage(NetIncomingMessage msg)
        {

        }
    }
}
