#nullable enable
using Microsoft.MixedReality.Volumetric;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CsBoids
{
    public class BoidsVolume
    {
        private readonly BoidManager _boidManager;
        private readonly VolumetricApp _volumetricApp;
        private Volume? _volume;
        private HandTracker? _handTracker;

        private List<Vector3> _targets = new List<Vector3>();
        private List<Vector3> _avoids = new List<Vector3>();
        private VaHandJointExt[] _jointsToTrack = { VaHandJointExt.Palm };//, VaHandJointExt.ThumbTip, VaHandJointExt.IndexTip, VaHandJointExt.MiddleTip, VaHandJointExt.RingTip, VaHandJointExt.LittleTip };

        private ModelResource? _modelResource;
        private List<VisualElement> _boids = new List<VisualElement>();

        internal BoidsVolume(string appName, BoidManager boidManager)
        {
            _volumetricApp = new VolumetricApp(appName,
                requiredExtensions: new string[] {
                    Extensions.VA_EXT_gltf2_model_resource,
                    Extensions.VA_EXT_locate_joints,
                    Extensions.VA_EXT_volume_container_modes});
            _volumetricApp.OnStart += OnStart;
            _volumetricApp.RunAsync();

            _boidManager = boidManager;
        }

        private void OnStart(VolumetricApp app)
        {
            _volume = new Volume(app);
            _volume.Content.SetSize(new VaExtent3Df
            {
                width = _boidManager.FlockRange,
                height = _boidManager.FlockRange,
                depth = _boidManager.FlockRange
            });
            _volume.Content.SetSizeBehavior(VaVolumeSizeBehavior.Fixed);
            _volume.Container.SetRotationLock(VaVolumeRotationLockFlags.X | VaVolumeRotationLockFlags.Z);
            _volume.Container.AllowInteractiveMode(true);
            _volume.Container.AllowUnboundedMode(true);
            _volume.OnReady += _ => OnReady();
            _volume.OnUpdate += _ => OnVolumeUpdate();
            _volume.OnClose += _ => OnClose();
            _volume.RequestUpdate(VaVolumeUpdateMode.FullFramerate);
        }

        private void OnReady()
        {
            _handTracker = new HandTracker(_volume!);

            _boidManager.Start();

            Uri uri = new Uri(VolumetricApp.GetAssetUri("boid.glb"));
            _modelResource = new ModelResource(_volume!, uri.AbsoluteUri);

            for (int i = 0; i < _boidManager.Boids.Count; ++i)
            {
                var boid = new VisualElement(_volume!, _modelResource);
                _boids.Add(boid);
            }
        }

        private void OnVolumeUpdate()
        {
            float seconds = (long)(_volume!.FrameState.frameTime) * 1e-9f;

            if (_handTracker != null && _handTracker.IsReady)
            {
                _handTracker.Update();
                _targets.Clear();
                _avoids.Clear();

                foreach (int side in new int[] { 0, 1 })
                {
                    var hand = _handTracker.JointLocations[side];
                    if (hand.IsTracked)
                    {
                        foreach (var joint in _jointsToTrack)
                        {
                            var jointPosition = hand.Pose(joint).position;
                            if (side == 0 || side == 1)
                            {
                                _targets.Add(new Vector3(jointPosition.x, jointPosition.y, jointPosition.z));
                            }
                            else
                            {
                                _avoids.Add(new Vector3(jointPosition.x, jointPosition.y, jointPosition.z));
                            }

                        }
                    }
                }
                _boidManager.SetTargetPositions(_targets, _avoids);
            }

            _boidManager.Update(seconds);

            for (int i = 0; i < _boids.Count; ++i)
            {
                var boid = _boidManager.Boids[i];
                var model = _boids[i];

                UpdateBoidElement(model, boid);
            }
        }

        private void OnClose()
        {
            _boidManager.Stop();
            _volumetricApp.RequestExit();  // Exit volumetric app
            App.CloseMainWindow();  // Close xaml window
        }

        private void UpdateBoidElement(VisualElement element, Boid boid)
        {
            var p = new VaVector3f()
            {
                x = boid.Position.X,
                y = boid.Position.Y,
                z = boid.Position.Z,
            };
            element.SetPosition(in p);

            var q = PitchYawRollToQuaternion(boid.Rotation);
            element.SetOrientation(in q);
        }

        private static VaQuaternionf PitchYawRollToQuaternion(System.Numerics.Vector3 euler)
        {
            float pitch = euler.X / 180.0f * MathF.PI;
            float yaw = euler.Y / 180.0f * MathF.PI;
            float roll = euler.Z / 180.0f * MathF.PI;

            // Calculate half-angles
            float halfPitch = pitch / 2.0f;
            float halfYaw = yaw / 2.0f;
            float halfRoll = roll / 2.0f;

            // Compute sine and cosine of half-angles
            float sinPitch = MathF.Sin(halfPitch);
            float cosPitch = MathF.Cos(halfPitch);
            float sinYaw = MathF.Sin(halfYaw);
            float cosYaw = MathF.Cos(halfYaw);
            float sinRoll = MathF.Sin(halfRoll);
            float cosRoll = MathF.Cos(halfRoll);

            // Calculate the quaternion components
            var q = new VaQuaternionf();
            q.x = sinPitch * cosYaw * cosRoll - cosPitch * sinYaw * sinRoll;
            q.y = cosPitch * sinYaw * cosRoll + sinPitch * cosYaw * sinRoll;
            q.z = cosPitch * cosYaw * sinRoll - sinPitch * sinYaw * cosRoll;
            q.w = cosPitch * cosYaw * cosRoll + sinPitch * sinYaw * sinRoll;

            return q;
        }
    }
}
