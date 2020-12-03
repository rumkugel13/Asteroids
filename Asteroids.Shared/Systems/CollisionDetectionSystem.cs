using Kadro.ECS;
using Kadro.Extensions;
using Kadro.Input;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Asteroids.Shared
{
    public class CollisionDetectionSystem : IUpdateSystem, IRenderSystem
    {
        public long DesiredComponentIds { get; } = 0;

        public bool UseSpatialHashing;

        private IndexedList<Entity> targets;
        private SpatialGrid<Entity> spatialGrid;
        private EntityWorld entityWorld;
        private HashSet<ulong> checkedPairs;
        private bool isDebug;

        public CollisionDetectionSystem(EntityWorld entityWorld)
        {
            this.entityWorld = entityWorld;
            this.UseSpatialHashing = true;
            this.targets = new IndexedList<Entity>();
            this.spatialGrid = new SpatialGrid<Entity>(GameConfig.CellSize, GameConfig.NumCells);
            this.checkedPairs = new HashSet<ulong>();

            this.DesiredComponentIds = entityWorld.ComponentManager.GetComponentId<CollidableComponent>() | entityWorld.ComponentManager.GetComponentId<TransformComponent>();
        }

        public void Initialize()
        {
            this.targets.Clear();
            this.isDebug = false;
        }

        public void OnEntityUpdate(Entity e)
        {
            if (this.IsInterested(e.GetComponentIds()) && !this.targets.Contains(e))
            {
                this.targets.Add(e);
            }
            else if (!this.IsInterested(e.GetComponentIds()) && this.targets.Contains(e))
            {
                this.targets.Remove(e);
            }
        }

        public bool IsInterested(long componentIds)
        {
            return (this.DesiredComponentIds & componentIds) == this.DesiredComponentIds;
        }

        public void Update(float elapsedSeconds)
        {
#if DEBUG
            if (Kadro.Input.KeyboardInput.OnKeyUp(Microsoft.Xna.Framework.Input.Keys.F2))
            {
                this.isDebug = !this.isDebug;
            }
#endif
            this.checkedPairs.Clear();

            if (this.UseSpatialHashing)
            {
                this.SpatialHashing();
            }
            else
            {
                this.CheckCollisions(this.targets);
            }
        }

        private void SpatialHashing()
        {
            this.spatialGrid.Initialize();
            
            // populate grid and apply transform to colliders
            for (int i = 0; i < this.targets.Count; i++)
            {
                CollidableComponent collidable = this.targets[i].GetComponent<CollidableComponent>();
                TransformComponent transform = this.targets[i].GetComponent<TransformComponent>();
                System.Diagnostics.Debug.Assert(collidable != null, "CollidableComponent not found");
                System.Diagnostics.Debug.Assert(transform != null, "TransformComponent not found");

                collidable.Collider.ApplyTransform(transform.Position, transform.Rotation, transform.Scale);
                this.spatialGrid.Add(collidable.Collider.GetBounds(), this.targets[i]);
            }

            //Parallel.For(0, this.spatialGrid.Length, c =>
            //{

            //WIP: still no unique collision check (also, perf. benefit?)
            //desc: loop over entities vs loop over cells
            ////for (int i = 0; i < this.targets.Count; i++)
            ////{
            ////    CollidableComponent collidable = this.targets[i].GetComponent<CollidableComponent>();
            ////    IndexedList<Entity> entities = this.spatialGrid.GetUniqueCellContents(collidable.Collider.GetBounds());
            ////    this.CheckCollision(this.targets[i], entities);
            ////    //this.CheckCollisions(entities);
            ////}

            for (int i = 0; i < this.spatialGrid.CellCount(); i++)
            {
                this.CheckCollisions(this.spatialGrid.GetCellContent(i));
            }

            //});
        }

        /// <summary>
        /// Checks all entites in the given list whether or not they intersect with each other
        /// </summary>
        /// <param name="entities">The list of entities</param>
        private void CheckCollisions(IReadOnlyList<Entity> entities)
        {
            for (int i = 0; i < entities.Count - 1; i++)
            {
                CollidableComponent collidable1 = entities[i].GetComponent<CollidableComponent>();
                TransformComponent transform1 = entities[i].GetComponent<TransformComponent>();
                System.Diagnostics.Debug.Assert(collidable1 != null, "CollidableComponent not found");
                System.Diagnostics.Debug.Assert(transform1 != null, "TransformComponent not found");

                //collidable1.Collider.ApplyTransform(transform1.Position, transform1.Rotation, transform1.Scale);
                Rectangle boundingRectangle1 = collidable1.Collider.GetBounds();

                for (int j = i + 1; j < entities.Count; j++)
                {
                    CollidableComponent collidable2 = entities[j].GetComponent<CollidableComponent>();
                    TransformComponent transform2 = entities[j].GetComponent<TransformComponent>();
                    System.Diagnostics.Debug.Assert(collidable2 != null, "CollidableComponent not found");
                    System.Diagnostics.Debug.Assert(transform2 != null, "TransformComponent not found");

                    // check if collision layers match
                    if (collidable1.CollisionLayer.Matches(collidable2.CollisionMask) || collidable2.CollisionLayer.Matches(collidable1.CollisionMask))
                    {
                        ulong keyPair = this.GetPairedKey(entities[i].EntityId, entities[j].EntityId);
                        // note: the following is necessary if spatial grid is being used
                        if (this.checkedPairs.Add(keyPair)) // check if this pair has already been processed
                        {
                            //collidable2.Collider.ApplyTransform(transform2.Position, transform2.Rotation, transform2.Scale);
                            Rectangle boundingRectangle2 = collidable2.Collider.GetBounds();

                            if (boundingRectangle1.Intersects(boundingRectangle2))  //broadphase, check if objects MAY collide
                            {
                                if (collidable1.Collider.Intersects(collidable2.Collider))  //narrowphase, (add missing colliders (line))
                                {
                                    collidable1.Collisions.Add(collidable2);
                                    collidable2.Collisions.Add(collidable1);
                                }
                            }
                        }
                    }
                }
            }
        }

        private ulong GetPairedKey(uint part1, uint part2)
        {
            return part1 < part2 ? ((ulong)part1 << 32) | part2 : ((ulong)part2 << 32) | part1;
        }

        public void Draw(float elapsedSeconds, SpriteBatch spriteBatch)
        {
            if (this.isDebug)
            {
                if (this.UseSpatialHashing)
                {
                    //note: test only
                    Kadro.Physics.PhysicsVisualizer.DrawSpatialGridCells(spriteBatch, this.spatialGrid.GetCells());

                    //todo: move this to another class (maybe even spatialgrid)
                    //for (int i = 0; i < GameConfig.NumCells.X; i++)
                    //{
                    //    spriteBatch.DrawLineSegment(new Vector2(i * GameConfig.CellSize.X, 0), new Vector2(i * GameConfig.CellSize.X, GameConfig.WorldSize.Y), Color.Blue, 1);
                    //}

                    //for (int i = 0; i < GameConfig.NumCells.Y; i++)
                    //{
                    //    spriteBatch.DrawLineSegment(new Vector2(0, i * GameConfig.CellSize.Y), new Vector2(GameConfig.WorldSize.X, i * GameConfig.CellSize.Y), Color.Blue, 1);
                    //}
                }

                for (int i = 0; i < this.targets.Count; i++)
                {
                    CollidableComponent collidable = this.targets[i].GetComponent<CollidableComponent>();
                    TransformComponent transform = this.targets[i].GetComponent<TransformComponent>();
                    System.Diagnostics.Debug.Assert(collidable != null, "CollidableComponent not found");
                    System.Diagnostics.Debug.Assert(transform != null, "TransformComponent not found");

                    collidable.Collider.ApplyTransform(transform.Position, transform.Rotation, transform.Scale);
                    collidable.DebugDraw(spriteBatch, 1);
                    spriteBatch.DrawLineSegment(transform.Position, transform.EntityToWorld(VectorExtensions.AngleToVector2(0f) * 20), Color.Green, 1);
                    spriteBatch.DrawLineSegment(transform.Position, transform.Position + Vector2.One, Color.Brown, 1);
                    //collidable.Collider.GetBounds().Draw(spriteBatch, Color.Violet, 1);
                }
            }
        }
    }
}
