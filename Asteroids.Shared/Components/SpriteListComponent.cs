using Kadro.ECS;
using Kadro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids.Shared
{
    public class SpriteListComponent : IComponent
    {
        public List<Sprite> Sprites;

        public SpriteListComponent()
        {
            this.Sprites = new List<Sprite>();
        }
    }
}
