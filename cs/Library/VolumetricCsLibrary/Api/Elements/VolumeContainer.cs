// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using System;
    using System.Collections.Generic;
    using Api = Detail.Api;

    /// <summary>
    /// VolumeContainer represents the container of the volume that can be used to manage its properties and modes.
    /// The volume container is the unit for the platform to manage the volume placement and behaviors.
    /// It allows setting properties such as display name, rotation lock, and allowed modes.
    /// It also provides events to notify when the volume enters or exits different modes such as interactive mode, sharing mode etc.
    /// </summary>
    public class VolumeContainer : Element
    {
        /// <summary>
        /// Creates a reference to the VolumeContainer in the specified volume.
        /// </summary>
        public VolumeContainer(Volume volume)
            : base(VaElementType.VolumeContainer, volume, CreateElement)
        {
        }

        /// <summary>
        /// Sets the display name of the volume container.
        /// The display name is used to represent the volume in the system UI.
        /// </summary>
        public void SetDisplayName(in string displayName)
        {
            if (displayName != _displayName)
            {
                _displayName = displayName;
                SetPropertyString(VaElementProperty.VolumeContainerDisplayName, displayName);
            }
        }

        /// <summary>
        /// Sets the rotation lock for the volume container.
        /// The rotation lock determines whether the volume can be rotated on any axis by the user.
        /// By default, the rotation lock is set to None, allowing the volume to be rotated freely.
        /// </summary>
        public void SetRotationLock(VaVolumeRotationLockFlags rotationLock)
        {
            if (rotationLock != _rotationLock)
            {
                _rotationLock = rotationLock;
                SetPropertyFlags(VaElementProperty.VolumeContainerRotationLock, (UInt32)rotationLock);
            }
        }

        /// <summary>
        /// Set the URI for the thumbnail model of the volume container.
        /// The thumbnail model is used to represent the volume in system UIs, such as the volume summary view.
        /// It should be a URI to a Gltf2 model that represents the volume.
        /// The model should be small and optimized for quick loading. Any gltf model > 500KB might be rejected and ignored.
        /// If this property is not set, set to null, set to an empty string or failed to load as gltf file,
        /// the platform will present the volume with a generic thumbnail 3D model.
        /// </summary>
        public void SetThumbnailModelUri(in string thumbnailModelUri)
        {
            if (thumbnailModelUri != _thumbnailModelUri)
            {
                _thumbnailModelUri = thumbnailModelUri;
                SetPropertyString(VaElementProperty.VolumeContainerThumbnailModelUriExt, thumbnailModelUri);
            }
        }

        /// <summary>
        /// Set the URI for the thumbnail icon of the volume container.
        /// The thumbnail icon is used to represent the volume in system UIs together with the display name.
        /// The thumbnail icon must be a square PNG image with size between 32x32 pixels to 256x256 pixels.
        /// By providing a 256px icon, it ensures the system only ever scales your icon down, never up.
        /// The image may have transparent pixels.
        /// If this property is not set, set to null, set to an empty string or failed to load as PNG file,
        /// the platform will present the volume with a generic thumbnail icon.
        /// </summary>
        public void SetThumbnailIconUri(in string thumbnailIconUri)
        {
            if (_thumbnailIconUri != thumbnailIconUri)
            {
                _thumbnailIconUri = thumbnailIconUri;
                SetPropertyString(VaElementProperty.VolumeContainerThumbnailIconUriExt, thumbnailIconUri);
            }
        }

        // <summary>
        // Set whether the volume is allowed to be in the interactive mode.
        // By default, it is disallowed and the user cannot switch to the interactive mode.
        // The hand inputs will only be delivered to the app when the user enters the interactive mode.
        // To properly handle hand inputs, the app must first allow this mode.
        // </summary>
        public void AllowInteractiveMode(bool allow)
        {
            var newFlags = allow
                ? _allowedModes | VaVolumeContainerModeFlagsExt.InteractiveMode
                : _allowedModes & ~VaVolumeContainerModeFlagsExt.InteractiveMode;
            SetCapabilityFlags(newFlags);
        }

        // <summary>
        // Set whether the volume is allowed to be in the one-to-one mode.
        // By default, it is allowed and the user can switch to the one-to-one mode.
        // If disallowed, the user cannot switch this volume to the one-to-one mode.
        // </summary>
        public void AllowOneToOneMode(bool allow)
        {
            var newFlags = allow
                ? _allowedModes | VaVolumeContainerModeFlagsExt.OneToOneMode
                : _allowedModes & ~VaVolumeContainerModeFlagsExt.OneToOneMode;
            SetCapabilityFlags(newFlags);
        }

        // <summary>
        // Set whether the volume is allowed to be shared in Teams.
        // By default, it is allowed and the user can switch to the shareable in Teams mode.
        // If disallowed, the user cannot share this volume in teams.
        // </summary>
        public void AllowSharingInTeams(bool allow)
        {
            var newFlags = allow
                ? _allowedModes | VaVolumeContainerModeFlagsExt.ShareableInTeams
                : _allowedModes & ~VaVolumeContainerModeFlagsExt.ShareableInTeams;
            SetCapabilityFlags(newFlags);
        }

        // <summary>
        // Set whether the volume is allowed to be in the unbounded mode.
        // By default, it is allowed and the user can switch to the unbounded mode.
        // If disallowed, the user cannot switch this volume to the unbounded mode.
        // </summary>
        public void AllowUnboundedMode(bool allow)
        {
            var newFlags = allow
                ? _allowedModes | VaVolumeContainerModeFlagsExt.UnboundedMode
                : _allowedModes & ~VaVolumeContainerModeFlagsExt.UnboundedMode;
            SetCapabilityFlags(newFlags);
        }

        // <summary>
        // Set whether the volume is allowed to be in the subpart mode.
        // By default, it is allowed and the user can switch to the subpart mode.
        // If disallowed, the user cannot switch this volume to the subpart mode.
        // </summary>
        public void AllowSubpartMode(bool allow)
        {
            var newFlags = allow
                ? _allowedModes | VaVolumeContainerModeFlagsExt.SubpartMode
                : _allowedModes & ~VaVolumeContainerModeFlagsExt.SubpartMode;
            SetCapabilityFlags(newFlags);
        }

        private void SetCapabilityFlags(VaVolumeContainerModeFlagsExt newFlags)
        {
            if (_allowedModes != newFlags)
            {
                _allowedModes = newFlags;
                SetPropertyFlags(VaElementProperty.VolumeContainerModeCapabilitiesExt, (UInt32)_allowedModes);
            }
        }

        /// <summary>
        // Notify when the volume enters or exits the interactive mode.
        /// </summary>
        public event Action<bool>? onInteractiveModeChanged;

        /// <summary>
        // Notify when the volume enters or exits the one-to-one mode.
        /// </summary>
        public event Action<bool>? onOneToOneModeChanged;

        /// <summary>
        /// Notify when the volume enters or exits the sharing mode in Teams.
        /// </summary>
        public event Action<bool>? onSharingInTeamsChanged;

        // <summary>
        // Notify when the volume enters or exits the unbounded mode.
        // </summary>
        public event Action<bool>? onUnboundedModeChanged;

        // <summary>
        // Notify when the volume enters or exits the subpart mode.
        // </summary>
        public event Action<bool>? onSubpartModeChanged;

        private string? _displayName;
        private string? _thumbnailModelUri;
        private string? _thumbnailIconUri;
        private VaVolumeRotationLockFlags _rotationLock = VaVolumeRotationLockFlags.None;

        private VaVolumeContainerModeFlagsExt _allowedModes = VaVolumeContainerModeFlagsExt.DefaultAllowed;
        private VaVolumeContainerModeFlagsExt _currentModes = VaVolumeContainerModeFlagsExt.None;

        private void SafeInvoke(Action<bool>? action, VaVolumeContainerModeFlagsExt modeFlagBit)
        {
            try
            {
                action?.Invoke(_currentModes.HasFlag(modeFlagBit));
            }
            catch (Exception e)
            {
                Detail.Trace.LogError(() => $"Exception when invoke for flag {modeFlagBit}: {e}");
            }
        }

        internal void HandleContainerModeChanged(in Api.VaEventVolumeContainerModeChangedExt changes)
        {
            if (_currentModes != (VaVolumeContainerModeFlagsExt)changes.currentModes)
            {
                VaVolumeContainerModeFlagsExt changedMode = _currentModes ^ (VaVolumeContainerModeFlagsExt)changes.currentModes;
                _currentModes = (VaVolumeContainerModeFlagsExt)changes.currentModes;

                if (changedMode.HasFlag(VaVolumeContainerModeFlagsExt.InteractiveMode))
                {
                    SafeInvoke(onInteractiveModeChanged, VaVolumeContainerModeFlagsExt.InteractiveMode);
                }
                if (changedMode.HasFlag(VaVolumeContainerModeFlagsExt.OneToOneMode))
                {
                    SafeInvoke(onOneToOneModeChanged, VaVolumeContainerModeFlagsExt.OneToOneMode);
                }
                if (changedMode.HasFlag(VaVolumeContainerModeFlagsExt.ShareableInTeams))
                {
                    SafeInvoke(onSharingInTeamsChanged, VaVolumeContainerModeFlagsExt.ShareableInTeams);
                }
                if (changedMode.HasFlag(VaVolumeContainerModeFlagsExt.UnboundedMode))
                {
                    SafeInvoke(onUnboundedModeChanged, VaVolumeContainerModeFlagsExt.UnboundedMode);
                }
                if (changedMode.HasFlag(VaVolumeContainerModeFlagsExt.SubpartMode))
                {
                    SafeInvoke(onSubpartModeChanged, VaVolumeContainerModeFlagsExt.SubpartMode);
                }
            }
        }
    }
}
