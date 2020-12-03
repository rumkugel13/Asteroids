using Kadro.ECS;
using System.Collections.Generic;

namespace Asteroids.Shared
{
    public class TagCountSystem : IEntitySystem
    {
        private long DesiredComponentIds = 0;

        public EntityWorld EntityWorld { get; private set; }

        private Dictionary<uint, Entity> entities;
        private Dictionary<string, int> countedTags;

        public TagCountSystem(EntityWorld entityWorld)
        {
            this.EntityWorld = entityWorld;
            this.entities = new Dictionary<uint, Entity>();
            this.countedTags = new Dictionary<string, int>();
            this.DesiredComponentIds = entityWorld.ComponentManager.GetComponentId<TagComponent>();
        }

        public void Initialize()
        {
            this.entities.Clear();
        }

        public bool IsInterested(long componentIds)
        {
            return (this.DesiredComponentIds & componentIds) == this.DesiredComponentIds;
        }

        public void OnEntityUpdate(Entity e)
        {
            if (this.IsInterested(e.GetComponentIds()) && !this.entities.ContainsKey(e.EntityId))
            {
                this.entities.Add(e.EntityId, e);

                string tag = e.GetComponent<TagComponent>().Tag;
                if (this.countedTags.ContainsKey(tag))
                {
                    this.countedTags[tag]++;
                }
                else
                {
                    this.countedTags.Add(tag, 1);
                }
            }
            else if (!this.IsInterested(e.GetComponentIds()) && this.entities.ContainsKey(e.EntityId))
            {
                this.entities.Remove(e.EntityId);

                string tag = e.GetComponent<TagComponent>().Tag;
                this.countedTags[tag]--;
            }
        }

        public int EntityCount()
        {
            return this.entities.Count;
        }

        public int EntityCount(string tag)
        {
            if (this.countedTags.ContainsKey(tag))
            {
                return this.countedTags[tag];
            }

            return 0;
        }
    }
}
