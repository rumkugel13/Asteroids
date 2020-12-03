using Kadro.ECS;

namespace Asteroids.Shared
{
    public class IntentComponent : IComponent
    {
        public IntentManager IntentManager;

        public IntentComponent(IntentManager intentManager)
        {
            this.IntentManager = intentManager;
        }
    }
}
