using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace CsBoids
{
    public class Boid
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Rotation { get; set; }
        public List<Boid> Neighbors { get; set; }

        public Boid(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
            Rotation = Vector3.Normalize(velocity);
        }

        public void UpdateBoid(Vector3 position, Vector3 velocity)
        {

            var direction = Vector3.Normalize(position - Position);
            Position = position;
            Velocity = velocity;

            float x = direction.X;
            float y = direction.Y;
            float z = direction.Z;
            float pitch = MathF.Asin(y);
            float yaw = MathF.Atan2(x, -z);
            float roll = 0; // assuming no roll

            Rotation = new Vector3(pitch, -yaw, roll) * 57.2957795f;
        }

        public void UpdateNeighbors(List<Boid> boids, float distance)
        {
            var neighbors = new List<Boid>();

            for (var i = 0; i < boids.Count; ++i)
            {
                if (Position != boids[i].Position)
                {
                    if (Vector3.Distance(boids[i].Position, Position) < distance)
                    {
                        neighbors.Add(boids[i]);
                    }
                }
            }
            Neighbors = neighbors;
        }

        private Vector3 GetClosest(List<Vector3> targets, Vector3 position)
        {
            Vector3 closestTarget = Vector3.Zero;
            foreach (var target in targets)
            {
                if (closestTarget == Vector3.Zero)
                {
                    closestTarget = target;
                }
                else
                {
                    if (Vector3.Distance(Position, target) < Vector3.Distance(Position, closestTarget))
                    {
                        closestTarget = target;
                    }
                }
            }
            return closestTarget;
        }

        public Vector3 Seek(List<Vector3> targets, float weight)
        {
            if (weight < 0.0001f)
            {
                return Vector3.Zero;
            }

            var desiredVelocity = Vector3.Normalize(GetClosest(targets, Position) - Position) * weight;
            return desiredVelocity - Velocity;
        }

        public Vector3 Avoid(List<Vector3> targets, float weight)
        {
            var c = Vector3.Zero;    // Center point of a move away from close targets

            for (var i = 0; i < targets.Count; ++i)
            {
                var target = targets[i];
                var distance = Vector3.Distance(Position, target);

                c = c + Vector3.Normalize(Position - target) / MathF.Pow(distance, 2);
            }
            return c * weight;
        }

        public Vector3 Cohesion(float steps, float weight)
        {
            var pc = Vector3.Zero;    // Perceived Center of Neighbors

            if (Neighbors.Count == 0)
            {
                return pc;
            }

            // Add up the positions of the neighbors
            for (var i = 0; i < Neighbors.Count; ++i)
            {
                var neighbor = Neighbors[i];
                if (pc == Vector3.Zero)
                {
                    pc = neighbor.Position;
                }
                else
                {
                    pc = pc + neighbor.Position;
                }
            }
            // Average the neighbor's positions
            pc = pc / Neighbors.Count;
            // Return the offset vector, divide by steps (100 would mean 1% towards center) and multiply by weight
            return (pc - Position) / steps * weight;
        }

        public Vector3 Separation(float weight)
        {
            var c = Vector3.Zero;    // Center point of a move away from close neighbors

            for (var i = 0; i < Neighbors.Count; ++i)
            {
                var neighbor = Neighbors[i];
                var distance = Vector3.Distance(Position, neighbor.Position);

                c = c + Vector3.Normalize(Position - neighbor.Position) / MathF.Pow(distance, 2);
            }
            return c * weight;
        }

        public Vector3 Alignment(float weight)
        {
            Vector3 pv = Vector3.Zero;    // Perceived Velocity of Neighbors

            if (Neighbors.Count == 0)
            {
                return pv;
            }

            for (var i = 0; i < Neighbors.Count; ++i)
            {
                var neighbor = Neighbors[i];
                pv = pv + neighbor.Velocity;
            }
            // Average the velocities
            if (Neighbors.Count > 1)
            {
                pv = pv / (Neighbors.Count);
            }
            // Normalize the offset vector and multiply by weight
            return (pv - Velocity) * weight;
        }

        public Vector3 Socialize(List<Boid> boids, float weight)
        {
            var pc = Vector3.Zero;    // Perceived Center of the rest of the flock

            if (Neighbors.Count != 0)
            {
                return pc;
            }

            // Add up the positions of all other boids
            for (var i = 0; i < boids.Count; ++i)
            {
                var boid = boids[i];
                if (Position != boid.Position)
                {
                    if (pc == Vector3.Zero)
                    {
                        pc = boid.Position;
                    }
                    else
                    {
                        pc = pc + boid.Position;
                    }
                }
            }
            // Average the positions
            if (boids.Count > 1)
            {
                pc = pc / (boids.Count - 1);
            }
            // Normalize the offset vector, divide by steps (100 would mean 1% towards center) and multiply by weight
            return Vector3.Normalize(pc - Position) * weight;
        }

        //----------
        // Arrival
        //----------
        public Vector3 Arrival(List<Vector3> targets, float slowingDistance, float maxSpeed)
        {
            var desiredVelocity = Vector3.Zero;
            if (slowingDistance < 0.0001f)
            {
                return desiredVelocity;
            }

            var target = GetClosest(targets, Position);

            var targetOffset = target - Position;
            var distance = Vector3.Distance(target, Position);
            var rampedSpeed = maxSpeed * (distance / slowingDistance);
            var clippedSpeed = MathF.Min(rampedSpeed, maxSpeed);
            if (distance > 0)
            {
                desiredVelocity = (clippedSpeed / distance) * targetOffset;
            }
            return desiredVelocity - Velocity;
        }

        public void PrintNeighbors()
        {
            Debug.Print("Neighbors: {0}", Neighbors.Count);
            for (var i = 0; i < Neighbors.Count; ++i)
            {
                var neighbor = Neighbors[i];
                Debug.Print("X: {0}  Y: {1}  Z: {2}", neighbor.Position.X, neighbor.Position.Y, neighbor.Position.Z);
            }
        }

        public Vector3 LimitVelocity(Vector3 v, float limitMax)
        {
            if (v.Length() > limitMax)
            {
                v = v / v.Length() * limitMax;
            }
            return v;
        }


        public Vector3 LimitRotation(Vector3 v, float maxAngle, float maxSpeed)
        {
            return Vector3.Lerp(Velocity, v, maxAngle);
            //return Vector3.RotateTowards(Velocity, v, maxAngle * MathF.Deg2Rad, maxSpeed);
        }
    }
}
