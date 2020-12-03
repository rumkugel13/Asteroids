using Asteroids.Shared;
using RKDnet;

namespace Asteroids
{
    public class ClientInputManager : InputManager
    {
        private BaseConnection connectionInfo;

        public ClientInputManager(BaseConnection connectionInfo, IntentManager intentManager) : base(intentManager)
        {
            this.connectionInfo = connectionInfo;
        }

        protected override void OnIntentChange(Intent intent, bool value)
        {
            base.OnIntentChange(intent, value);
            this.connectionInfo.SendReliablePacket(new IntentPacket() { Intent = (byte)intent, IsSet = value });
        }
    }
}
