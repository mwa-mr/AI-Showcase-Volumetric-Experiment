// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

using Microsoft.MixedReality.Volumetric;

// This wrapper class takes Unity's vector data type instead and make sure converted before calling APIs.
public class UnityVolume : Volume
{
    public UnityEngine.Vector3 CurrentSize { get; private set; } = UnityEngine.Vector3.one;
    public UnityEngine.Vector3 CurrentPosition { get; private set; } = UnityEngine.Vector3.zero;
    public UnityEngine.Quaternion CurrentRotation { get; private set; } = UnityEngine.Quaternion.identity;

    public UnityVolume(VolumetricApp app)
        : base(app)
    {
    }

    public void SetContentPosition(UnityEngine.Vector3 position)
    {
        if (position != CurrentPosition)
        {
            CurrentPosition = position;
            Content.SetPosition(position.ToVolumetricPos());
        }
    }

    public void SetContentSize(UnityEngine.Vector3 size)
    {
        if (size != CurrentSize)
        {
            CurrentSize = size;
            Content.SetSize(size.ToVolumetricSize());
        }
    }

    public void SetContentRotation(UnityEngine.Quaternion rotation)
    {
        if (rotation != CurrentRotation)
        {
            CurrentRotation = rotation;
            Content.SetOrientation(rotation.ToVolumetricRot());
        }
    }
}
