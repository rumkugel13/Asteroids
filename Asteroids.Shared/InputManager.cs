using Asteroids.Shared;
using Kadro.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Asteroids.Shared
{
    public class InputManager
    {
        private IntentManager intentManager;
        private Keys[] keyMapping;
        private Keys[] alternateKeyMapping;

        public InputManager(IntentManager intentManager)
        {
            this.intentManager = intentManager;
            this.keyMapping = new Keys[(int)Intent.Size];
            this.alternateKeyMapping = new Keys[(int)Intent.Size];
            this.SetKeyMapping();
        }

        private void SetKeyMapping()
        {
            this.keyMapping[(int)Intent.Accelerate] = Keys.Up;
            this.keyMapping[(int)Intent.Decelerate] = Keys.Down;
            this.keyMapping[(int)Intent.RotateLeft] = Keys.Left;
            this.keyMapping[(int)Intent.RotateRight] = Keys.Right;
            this.keyMapping[(int)Intent.Shoot] = Keys.Space;
            this.keyMapping[(int)Intent.ShootMissile] = Keys.RightShift;

            this.alternateKeyMapping[(int)Intent.Accelerate] = Keys.W;
            this.alternateKeyMapping[(int)Intent.Decelerate] = Keys.S;
            this.alternateKeyMapping[(int)Intent.RotateLeft] = Keys.A;
            this.alternateKeyMapping[(int)Intent.RotateRight] = Keys.D;
            this.alternateKeyMapping[(int)Intent.Shoot] = Keys.Space;
            this.alternateKeyMapping[(int)Intent.ShootMissile] = Keys.RightShift;
        }

        public void Update()
        {
            for (int i = 0; i < this.keyMapping.Length; i++)
            {
                if (Kadro.Input.KeyboardInput.OnKeyChange(this.keyMapping[i]))
                {
                    this.OnIntentChange((Intent)i, Kadro.Input.KeyboardInput.IsKeyDown(this.keyMapping[i]));
                }
            }

            for (int i = 0; i < this.alternateKeyMapping.Length; i++)
            {
                if (Kadro.Input.KeyboardInput.OnKeyChange(this.alternateKeyMapping[i]))
                {
                    this.OnIntentChange((Intent)i, Kadro.Input.KeyboardInput.IsKeyDown(this.alternateKeyMapping[i]));
                }
            }
        }

        protected virtual void OnIntentChange(Intent intent, bool value)
        {
            this.intentManager.SetIntent(intent, value);
        }
    }
}
