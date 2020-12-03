using Kadro.ECS;
using Microsoft.Xna.Framework;

namespace Asteroids.Shared
{
    public class MotionComponent : IComponent
    {
        // linear properties
        public Vector2 Velocity;

        public float MaxVelocity;

        public Vector2 Acceleration;

        public float Drag;

        ////public float Mass;
        
        ////public Vector2 Force;

        // angular properties
        public float AngularVelocity;

        public float MaxAngularVelocity;

        public float AngularAcceleration;

        public float AngularDrag;

        ////public float Inertia;

        ////public float Torque;

        public MotionComponent(Vector2 velocity, float angularVelocity, float maxVelocity, float maxRotation)
        {
            this.Velocity = velocity;
            this.AngularVelocity = angularVelocity;
            this.MaxVelocity = maxVelocity;
            this.MaxAngularVelocity = maxRotation;
            ////this.Mass = 1.0f;
            ////this.Inertia = 1.0f;
        }

        ////public void Integrate(float deltaTime)
        ////{
        ////    //integrate physics
        ////    //linear
        ////    if (this.Force != 0)
        ////    {        
        ////        Vector2 acceleration = this.Force / this.Mass;
        ////    }
        ////    this.Velocity += acceleration * deltaTime;
        ////    this.Position += this.Velocity * deltaTime;
        ////    this.Forces = Vector2.Zero; //clear forces

        ////    //angular
        ////    float angAcc = this.Torque / this.Inertia;
        ////    this.AngularVelocity += angAcc * deltaTime;
        ////    this.Rotation += this.AngularVelocity * deltaTime;
        ////    this.Torque = 0; //clear torque
        ////}

        ////public static float GetRectangleInertia(float mass, Vector2 halfSize)
        ////{
        ////    return (1.0f / 12.0f) * (halfSize.X * halfSize.X) * (halfSize.Y * halfSize.Y) * mass;
        ////}

        ////public static float GetCircleInertia(float mass, float radius)
        ////{
        ////    return (1.0f / 2.0f) * (radius * radius) * mass;
        ////}

        public void ResetForces()
        {
            this.Acceleration = Vector2.Zero;
            this.AngularAcceleration = 0f;
        }

        public void AddAcceleration(Vector2 acceleration)
        {
            this.Acceleration += acceleration;
        }

        // Add force to offset from center of mass
        ////public void AddAcceleration(Vector2 acceleration, Vector2 offset)
        ////{
        ////    this.Acceleration += acceleration;
        ////    this.AngularAcceleration += VectorExtensions.Cross(offset, acceleration);
        ////}

        public void AddAngularAcceleration(float angularAcceleration)
        {
            this.AngularAcceleration += angularAcceleration;
        }

        // Add force to center of mass
        ////public void AddForce(Vector2 force)
        ////{
        ////    this.Acceleration += force / this.Mass;
        ////}

        // Add force to offset from center of mass
        ////public void AddForce(Vector2 force, Vector2 offset)
        ////{
        ////    this.Acceleration += force / this.Mass;
        ////    this.AngularAcceleration += VectorExtensions.Cross(offset, force) / this.Inertia;
        ////}

        // Add torque directly
        ////public void AddTorque(float torque)
        ////{
        ////    this.AngularAcceleration += torque / this.Inertia;
        ////}
    }
}
