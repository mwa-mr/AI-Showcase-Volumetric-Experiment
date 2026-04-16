using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace CsBoids
{
    public class BoidManager
    {
        private List<Vector3> Targets = new List<Vector3>() { Vector3.Zero };
        private List<Vector3> Avoids = new List<Vector3>() { Vector3.Zero };

        public int NumberOfBoids { get; private set; } = 30;
        public float NeighborDistance { get; set; } = 5.0f;
        public float MaxVelocty { get; set; } = 15.0f;
        public float MinVelocty { get; set; } = 0.05f;
        public float MaxRotationAngle { get; set; } = .001f;
        public Vector3 InitialVelocity { get; private set; } = new Vector3(0, 0.1f, 0);
        public float ArrivalSlowingDistance { get; set; } = 5.0f;
        public float ArrivalMaxSpeed { get; set; } = 8.0f;
        public float CohesionStep { get; private set; } = 100;
        public float CohesionWeight { get; set; } = 0.5f;
        public float SeparationWeight { get; set; } = 0.25f;
        public float AlignmentWeight { get; set; } = 0.2f;
        public float SeekWeight { get; set; } = .5f;
        public float AvoidWeight { get; private set; } = 35.0f;
        public float SocializeWeight { get; set; } = .6f;
        public float FlockRange { get; private set; } = 15;

        public List<Boid> Boids { get; private set; } = new List<Boid>();


        private bool useExternalTarget;
        private long lastExternalTargetUpdate;

        public BoidManager()
        {
        }

        public void Start()
        {
            if (sw.IsRunning)
            {
                return;
            }

            sw.Reset();
            Boids.Clear();
            for (var i = 0; i < NumberOfBoids; ++i)
            {
                var position = new Vector3(
                    new Random().NextSingle() * FlockRange - FlockRange / 2,
                    new Random().NextSingle() * FlockRange - FlockRange / 2,
                    new Random().NextSingle() * FlockRange - FlockRange / 2);
                var boid = new Boid(position, InitialVelocity * Vector3.Normalize(position));
                Boids.Add(boid);
            }

            for (var i = 0; i < Boids.Count; ++i)
            {
                Boids[i].UpdateNeighbors(Boids, NeighborDistance);
            }
            sw.Start();
        }

        public void Stop()
        {
            sw.Stop();
        }

        public void SetTargetPositions(List<Vector3> positions, List<Vector3> avoids)
        {
            if (positions.Count == 0 && avoids.Count == 0)
            {
                return;
            }

            Targets.Clear();
            Avoids.Clear();
            Targets.AddRange(positions);
            Avoids.AddRange(avoids);
            useExternalTarget = true;
            lastExternalTargetUpdate = sw.ElapsedMilliseconds;
        }

        public void Update(float elapsedSec)
        {
            UpdateBoids(elapsedSec);
        }

        Stopwatch sw = new Stopwatch();
        bool updating;
        uint tick;
        private void UpdateBoids(float elapsedSec)
        {
            if (updating || sw.ElapsedMilliseconds < 1000)
            {
                return;
            }

            updating = true;
            useExternalTarget = sw.ElapsedMilliseconds - lastExternalTargetUpdate < 2500;
            if (!useExternalTarget)
            {
                if (tick++ % 150 == 0)
                {
                    Targets.Clear();
                    Avoids.Clear();

                    Targets.Add(.33f * new Vector3(
                        new Random().NextSingle() * FlockRange,
                        new Random().NextSingle() * FlockRange,
                        new Random().NextSingle() * FlockRange));

                    Avoids.Add(.33f * new Vector3(
                        new Random().NextSingle() * FlockRange,
                        new Random().NextSingle() * FlockRange,
                        new Random().NextSingle() * FlockRange));
                }
            }

            for (var i = 0; i < Boids.Count; ++i)
            {
                var boid = Boids[i];
                // Update its neighbors within a distance
                boid.UpdateNeighbors(Boids, NeighborDistance);
                // Steering Behaviors
                var cohesionVector = boid.Cohesion(CohesionStep, CohesionWeight);
                var separationVector = boid.Separation(SeparationWeight);
                var alignmentVector = boid.Alignment(AlignmentWeight);
                var seekVector = boid.Seek(Targets, SeekWeight);
                var avoidVector = boid.Avoid(Avoids, AvoidWeight);
                var socializeVector = boid.Socialize(Boids, SocializeWeight);
                var arrivalVector = boid.Arrival(Targets, ArrivalSlowingDistance, ArrivalMaxSpeed) * SeekWeight;
                // Update Boid's Position and Velocity
                var velocity = boid.Velocity + cohesionVector + separationVector + alignmentVector + seekVector + avoidVector + socializeVector + arrivalVector;
                velocity = boid.LimitVelocity(velocity, MaxVelocty);
                velocity = boid.LimitRotation(velocity, MaxRotationAngle, MaxVelocty);
                if (velocity.Length() < MinVelocty)
                {
                    velocity = Vector3.Normalize(velocity) * MinVelocty;
                }
                var position = boid.Position + velocity;
                boid.UpdateBoid(position, velocity);
            }
            updating = false;
        }
    }
}
