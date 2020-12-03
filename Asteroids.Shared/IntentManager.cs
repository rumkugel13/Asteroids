using System;

namespace Asteroids.Shared
{
    public enum Intent
    {
        Accelerate, Decelerate, RotateLeft, RotateRight, Shoot,
        ShootMissile, Size,
    }

    public class IntentManager
    {
        private bool[] intents;

        public IntentManager()
        {
            this.intents = new bool[(int)Intent.Size];
        }

        public void SetIntent(Intent intent, bool value)
        {
            this.intents[(int)intent] = value;
        }

        public bool HasIntent(Intent intent)
        {
            return this.intents[(int)intent];
        }

        public void Initialize()
        {
            for (int i = 0; i < this.intents.Length; i++)
            {
                this.intents[i] = false;
            }
        }
    }
}
