// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

using System;

namespace Microsoft.MixedReality.Volumetric
{
    /// <summary>
    /// VolumeContent represents the root of the volume content.
    /// All visual elements in the volume are placed in the volume content space.
    /// This volume content element can be used for the application to control the content placement, size and behavior.
    /// </summary>
    public class VolumeContent : Element
    {
        /// <summary>
        /// Creates a reference to volume content element in the specified volume.
        /// </summary>
        public VolumeContent(Volume volume)
            : base(VaElementType.VolumeContent, volume, CreateElement)
        {
        }

        /// <summary>
        /// Gets the volume content position
        /// </summary>
        public VaVector3f Position => _position;

        /// <summary>
        /// Sets the volume content position.
        /// </summary>
        public void SetPosition(in VaVector3f position)
        {
            if (position.x != _position.x ||
                position.y != _position.y ||
                position.z != _position.z)
            {
                _position = position;
                SetPropertyVector3f(VaElementProperty.VolumeContentPosition, position);
            }
        }

        /// <summary>
        /// Gets the volume content orientation.
        /// </summary>
        public VaQuaternionf Orientation => _orientation;

        /// <summary>
        /// Sets the volume content orientation.
        /// </summary>
        public void SetOrientation(in VaQuaternionf orientation)
        {
            if (orientation.x != _orientation.x ||
                orientation.y != _orientation.y ||
                orientation.z != _orientation.z ||
                orientation.w != _orientation.w)
            {
                _orientation = orientation;
                SetPropertyQuaternionf(VaElementProperty.VolumeContentOrientation, orientation);
            }
        }

        /// <summary>
        /// Gets the volume content size.
        /// </summary>
        public VaExtent3Df Size => _size;

        /// <summary>
        /// Sets the volume content size.
        /// When the size behavior is set to AutoSize, this set value will be ignored,
        /// </summary>
        public void SetSize(in VaExtent3Df size)
        {
            if (size.width != _size.width ||
                size.height != _size.height ||
                size.depth != _size.depth)
            {
                _size = size;
                SetPropertyExtent3Df(VaElementProperty.VolumeContentSize, size);
            }
        }

        /// <summary>
        /// Sets the volume content size with a uniform size for width, height, and depth.
        /// </summary>
        public void SetSize(float uniformSize)
        {
            SetSize(new VaExtent3Df() { width = uniformSize, height = uniformSize, depth = uniformSize });
        }

        /// <summary>
        /// Gets the volume content size behavior.
        /// </summary>
        public VaVolumeSizeBehavior SizeBehavior => _sizeBehavior;

        /// <summary>
        /// Sets the volume content size behavior.
        /// </summary>
        public void SetSizeBehavior(VaVolumeSizeBehavior sizeBehavior)
        {
            if (sizeBehavior != _sizeBehavior)
            {
                _sizeBehavior = sizeBehavior;
                SetPropertyEnum(VaElementProperty.VolumeContentSizeBehavior, (Int32)sizeBehavior);
            }
        }

        /// <summary>
        /// Gets the actual scale of the volume content.
        /// This can be different from the set scale when the size behavior is set to AutoSize,
        /// </summary>
        public float ActualScale =>
            GetPropertyFloat(VaElementProperty.VolumeContentActualScaleExt);

        /// <summary>
        /// Gets the actual position of the volume content.
        /// This can be different from the set position when the size behavior is set to AutoSize,
        /// </summary>
        public VaVector3f ActualPosition =>
            GetPropertyVector3f(VaElementProperty.VolumeContentActualPositionExt);

        /// <summary>
        /// Gets the actual orientation of the volume content.
        /// This can be different from the set orientation when the size behavior is set to AutoSize,
        /// </summary>
        public VaExtent3Df ActualSize =>
            GetPropertyExtent3Df(VaElementProperty.VolumeContentActualSizeExt);

        private VaVector3f _position = VaMath.Zero;
        private VaQuaternionf _orientation = VaMath.Identity;
        private VaExtent3Df _size = VaMath.ZeroSize;
        private VaVolumeSizeBehavior _sizeBehavior = VaVolumeSizeBehavior.AutoSize;
    }
}
