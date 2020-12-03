
namespace Asteroids.Shared
{
    public struct Signal
    {
        public uint EntityId;

        public SignalType SignalType;

        public Signal(SignalType signalType, uint entityId)
        {
            this.EntityId = entityId;
            this.SignalType = signalType;
        }
    }
}
