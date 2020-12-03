using Kadro.ECS;

namespace Asteroids.Shared
{
    public class TargetComponent : IComponent
    {
        public uint TargetId;

        public TargetComponent(uint targetId)
        {
            this.TargetId = targetId;
        }
    }
}
