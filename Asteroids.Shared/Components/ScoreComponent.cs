using Kadro.ECS;

namespace Asteroids.Shared
{
    public class ScoreComponent : IComponent
    {
        public int Value;

        public ScoreComponent(int score)
        {
            this.Value = score;
        }
    }
}
