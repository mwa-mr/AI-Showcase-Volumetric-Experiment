using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

internal enum PinchState
{
    Idle,
    Pinching,
}

internal class HandInteractionManager
{
    private HandTracker? _handTracker;
    private readonly PinchState[] _pinchStates = [PinchState.Idle, PinchState.Idle];
    private readonly float[] _lastSpawnTimes = [float.MinValue, float.MinValue];

    public event Action<VaVector3f>? OnPinchReleased;

    public void Create(Volume volume)
    {
        _handTracker = new HandTracker(volume);
    }

    public void Update(float currentTime, VaExtent3Df volumeSize, IReadOnlyList<SpawnedShape> activeShapes)
    {
        if (_handTracker?.IsReady != true) return;
        _handTracker.Update();

        for (int side = 0; side < 2; side++)
        {
            var hand = _handTracker.JointLocations[side];
            UpdatePinch(side, hand, currentTime, volumeSize);
            CheckPokes(hand, activeShapes);
        }
    }

    private void UpdatePinch(int side, JointLocations hand, float currentTime, VaExtent3Df volumeSize)
    {
        if (!hand.IsTracked)
        {
            _pinchStates[side] = PinchState.Idle;
            return;
        }

        var thumbTip = hand.Pose(VaHandJointExt.ThumbTip).position;
        var indexTip = hand.Pose(VaHandJointExt.IndexTip).position;
        float distance = Distance(thumbTip, indexTip);

        switch (_pinchStates[side])
        {
            case PinchState.Idle:
                if (distance < Constants.PinchStartThreshold)
                    _pinchStates[side] = PinchState.Pinching;
                break;

            case PinchState.Pinching:
                if (distance > Constants.PinchReleaseThreshold)
                {
                    _pinchStates[side] = PinchState.Idle;
                    if (currentTime - _lastSpawnTimes[side] > Constants.PinchCooldown)
                    {
                        var spawnPos = Midpoint(thumbTip, indexTip);
                        if (IsInsideVolume(spawnPos, volumeSize))
                        {
                            OnPinchReleased?.Invoke(spawnPos);
                            _lastSpawnTimes[side] = currentTime;
                        }
                    }
                }
                break;
        }
    }

    private static void CheckPokes(JointLocations hand, IReadOnlyList<SpawnedShape> activeShapes)
    {
        if (!hand.IsTracked) return;

        var indexTip = hand.Pose(VaHandJointExt.IndexTip).position;

        foreach (var shape in activeShapes)
        {
            if (shape.State != ShapeState.Alive) continue;

            float dist = Distance(indexTip, shape.Position);
            if (dist < Constants.PokeThreshold + Constants.ShapeSize / 2f)
            {
                shape.BeginDestroy();
            }
        }
    }

    private static bool IsInsideVolume(VaVector3f pos, VaExtent3Df size)
    {
        float hw = size.width / 2f;
        float hh = size.height / 2f;
        float hd = size.depth / 2f;
        return pos.x >= -hw && pos.x <= hw
            && pos.y >= -hh && pos.y <= hh
            && pos.z >= -hd && pos.z <= hd;
    }

    private static float Distance(VaVector3f a, VaVector3f b)
    {
        float dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static VaVector3f Midpoint(VaVector3f a, VaVector3f b)
    {
        return new VaVector3f
        {
            x = (a.x + b.x) / 2f,
            y = (a.y + b.y) / 2f,
            z = (a.z + b.z) / 2f,
        };
    }
}
