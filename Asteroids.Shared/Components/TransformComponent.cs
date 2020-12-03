using Kadro.ECS;
using Microsoft.Xna.Framework;
using System;

namespace Asteroids.Shared
{
    public class TransformComponent : IComponent
    {
        private Matrix transformMatrix, inverseMatrix;
        private Vector2 position, scale;
        private float rotation;
        private bool isDirty;

        public Vector2 Position
        {
            get
            {
                return this.position;
            }

            set
            {
                if (value != this.position)
                {
                    this.SetDirtyFlag();
                    this.position = value;
                }
            }
        }

        public float Rotation
        {
            get
            {
                return this.rotation;
            }

            set
            {
                if (this.rotation != value)
                {
                    this.SetDirtyFlag();
                    this.rotation = value;
                }
            }
        }

        public Vector2 Scale
        {
            get
            {
                return this.scale;
            }

            set
            {
                if (this.scale != value)
                {
                    this.SetDirtyFlag();
                    this.scale = value;
                }
            }
        }

        public Matrix TransformMatrix
        {
            get
            {
                if (this.isDirty)
                {
                    this.transformMatrix = this.UpdateMatrix();
                    this.inverseMatrix = Matrix.Invert(this.transformMatrix);
                }
                return this.transformMatrix;
            }

            private set => this.transformMatrix = value;
        }

        public Matrix InverseMatrix
        {
            get
            {
                if (this.isDirty)
                {
                    this.transformMatrix = this.UpdateMatrix();
                    this.inverseMatrix = Matrix.Invert(this.transformMatrix);
                }
                return this.inverseMatrix;
            }

            private set => this.inverseMatrix = value;
        }

        public TransformComponent(Vector2 position, float rotation, Vector2 scale)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
        }

        public Vector2 EntityToWorld(Vector2 position)
        {
            return Vector2.Transform(position, this.TransformMatrix);
        }

        public Vector2 WorldToEntity(Vector2 position)
        {
            return Vector2.Transform(position, this.InverseMatrix);
        }

        private void SetDirtyFlag()
        {
            this.isDirty = true;
        }

        private Matrix UpdateMatrix()
        {
            this.isDirty = false;
            return
                    Matrix.CreateScale(new Vector3(this.Scale, 1)) * 
                    Matrix.CreateRotationZ(this.Rotation) * 
                    Matrix.CreateTranslation(new Vector3(this.Position, 0.0f));
        }
    }
}
