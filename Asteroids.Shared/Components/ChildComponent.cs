using Kadro.ECS;
using System.Collections.Generic;

namespace Asteroids.Shared
{
    public class ChildComponent : IComponent
    {
        public List<uint> Children { get; protected set; }

        public ChildComponent()
        {
            this.Children = new List<uint>();
        }
    }
}
