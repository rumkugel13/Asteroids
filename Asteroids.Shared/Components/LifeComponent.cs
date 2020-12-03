using Kadro.ECS;

namespace Asteroids.Shared
{
    public enum LifeLostAction
    {
        Destroy, Respawn, 
    }

    public class LifeComponent : IComponent
    {
        public int Lives;

        public LifeLostAction OnLifeLost;

        public LifeLostAction OnZeroLives;

        public LifeComponent(int lives)
        {
            this.Lives = lives;
        }
    }
}
