using Kadro.ECS;
using Kadro.UI;

namespace Asteroids.Shared
{
    public class ScoreboardSystem : EntityUpdateSystem
    {
        private Scoreboard scoreboard;

        public ScoreboardSystem(EntityWorld entityWorld, Scoreboard scoreboard) : base(entityWorld,
            entityWorld.ComponentManager.GetComponentId<ScoreComponent>() |
            entityWorld.ComponentManager.GetComponentId<LifeComponent>())
        {
            this.scoreboard = scoreboard;
        }

        public override void Update(float elapsedSeconds)
        {
            foreach (Entity e in this.actives.Values)
            {
                ScoreComponent score = e.GetComponent<ScoreComponent>();
                LifeComponent life = e.GetComponent<LifeComponent>();

                System.Diagnostics.Debug.Assert(score != null, "ScoreComponent not found");
                System.Diagnostics.Debug.Assert(life != null, "LifeComponent not found");

                this.scoreboard.AddOrUpdate(e.EntityId, score.Value, life.Lives);
            }
        }
    }
}
