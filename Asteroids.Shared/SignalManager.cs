using System.Collections.Generic;

namespace Asteroids.Shared
{
    public enum SignalType
    {
        LifeLost, PointScored, EntityDestroyed, EntityCreated,
    }

    public class SignalManager
    {
        private Queue<Signal> signals;

        public SignalManager()
        {
            this.signals = new Queue<Signal>();
        }

        public void AddSignal(SignalType signalType, uint entityId)
        {
            Signal signal = new Signal(signalType, entityId);
            if (!this.signals.Contains(signal))
            {
                this.signals.Enqueue(signal);
            }
        }

        public bool HasSignals()
        {
            return this.signals.Count > 0;
        }

        public Signal GetNextSignal()
        {
            return this.signals.Dequeue();
        }
    }
}
