using Kadro.ECS;

namespace Asteroids.Shared
{
    public class TagComponent : IComponent
    {
        public string Tag;

        public TagComponent(string tag)
        {
            this.Tag = tag;
        }
    }
}
