using Kadro.ECS;

namespace Asteroids.Shared
{
    public class ParentComponent : IComponent
    {
        public uint ParentId;

        public ParentComponent(uint parentId)
        {
            this.ParentId = parentId;
        }
    }
}
