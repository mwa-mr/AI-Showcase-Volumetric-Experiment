// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using Detail;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// SpaceLocation represents the location of a space.
    /// It contains the pose of the space and a flag indicating whether the space is currently tracked.
    /// </summary>
    public struct SpaceLocation
    {
        /// <summary>
        /// Gets the pose of the space, which includes position and orientation.
        /// </summary>
        public VaPosef pose { get; internal set; }

        /// <summary>
        /// Gets whether the space is currently tracked.
        /// When true, the pose is actively updated by the tracking system.
        /// When false, the pose may not be actively tracked.
        /// </summary>
        public bool isTracked { get; internal set; }
    }

    /// <summary>
    /// SpaceLocations contains the locations of different spaces in the volume content space.
    public class SpaceLocations
    {
        /// <summary>
        /// Gets the location of the volume container space in the volume origin.
        /// The position of the volume container space is always at the center of the volume container,
        /// The rotation of the volume container space is always aligned with the volume container bounding box.
        /// The user or the platform controls the move and rotation of the container in the world.
        /// </summary>
        public SpaceLocation volumeContainer { get; internal set; }

        /// <summary>
        /// Gets the location of the volume content space in the volume origin.
        /// The volume content space is the space where all visual elements are placed in the volume.
        /// When the volume content is auto sized, the volume content space might be moved offcenter to fit the content.
        /// The application can change the volume content space through Volume.VolumeContent interface.
        /// </summary>
        public SpaceLocation volumeContent { get; internal set; }

        /// <summary>
        /// Gets the location of the viewer space in the volume content space.
        /// The viewer space is tracking the user's head motion and is updated by the platform.
        /// The -Z axis points forward of the user and it is in horizontal plane when user natually looks forward.
        /// The Y axis is aligned with the user's head and points upwards.
        /// The position of the viewer space is always at the center point of the user's eyes,
        /// </summary>
        public SpaceLocation viewer { get; internal set; }

        /// <summary>
        /// Gets the location of the local space in the volume content space.
        /// The local space is a reference point at eye level in when the user naturally looks forward.
        /// The Y axis is gravity aligned and points upwards.
        /// The -Z axis points forward as the user defined in Mixed Reality experience setup.
        /// The pose of the local space is defined by the platform and shared across all volumes and sessions.
        /// This space is in stationary frame of reference and is not affected by the user's or the volume's movement.
        /// </summary>
        public SpaceLocation local { get; internal set; }
    };

    /// <summary>
    /// SpaceLocator is used to locate different spaces in the volume content space.
    /// It provides access to the locations of the volume container, volume content, and viewer spaces.
    /// </summary>
    public class SpaceLocator : Element
    {
        private readonly SpaceLocations _locations = new SpaceLocations();

        private static readonly VaSpaceTypeExt[] m_spaces = {
            VaSpaceTypeExt.VolumeContainer,
            VaSpaceTypeExt.VolumeContent,
            VaSpaceTypeExt.Viewer,
            VaSpaceTypeExt.Local,
        };

        /// <summary>
        /// Creates a new SpaceLocator in the specified volume.
        /// </summary>
        public SpaceLocator(Volume volume)
            : base(VaElementType.SpaceLocatorExt, volume, CreateElement)
        {
        }

        /// <summary>
        /// Gets the locations of various spaces in the volume.
        /// </summary>
        public SpaceLocations Locations => _locations;

        /// <summary>
        /// Updates the locations of the spaces in the volume content space.
        /// Applications typically call this method on each update to refresh the locations of the spaces.
        /// </summary>
        public void Update()
        {
            Api.VaSpaceLocateInfoExt locateInfo = new();
            locateInfo.type = Api.VaStructureType.VA_TYPE_SPACE_LOCATE_INFO_EXT;
            locateInfo.baseSpace = Api.VaSpaceTypeExt.VA_SPACE_TYPE_VOLUME_CONTENT_EXT;
            locateInfo.spaceCount = (uint)m_spaces.Length;

            Api.VaSpaceLocationsExt locations = new();
            locations.type = Api.VaStructureType.VA_TYPE_SPACE_LOCATIONS_EXT;
            locations.locationCount = (uint)m_spaces.Length;

            IntPtr spacesBuffer = Marshal.AllocHGlobal(sizeof(VaSpaceTypeExt) * m_spaces.Length);
            IntPtr locationsBuffer = Marshal.AllocHGlobal(Marshal.SizeOf<VaSpaceLocationExt>() * m_spaces.Length);
            try
            {
                for (int i = 0; i < m_spaces.Length; i++)
                {
                    IntPtr dest = IntPtr.Add(spacesBuffer, i * sizeof(VaSpaceTypeExt));
                    Marshal.WriteInt32(dest, (int)m_spaces[i]);
                }

                locateInfo.spaces = spacesBuffer;
                locations.locations = locationsBuffer;
                Api.CheckResult(Api.vaLocateSpacesExt(Handle, locateInfo, out locations));

                _locations.volumeContainer = ReadSpaceLocation(locationsBuffer + 0 * Marshal.SizeOf<VaSpaceLocationExt>());
                _locations.volumeContent = ReadSpaceLocation(locationsBuffer + 1 * Marshal.SizeOf<VaSpaceLocationExt>());
                _locations.viewer = ReadSpaceLocation(locationsBuffer + 2 * Marshal.SizeOf<VaSpaceLocationExt>());
                _locations.local = ReadSpaceLocation(locationsBuffer + 3 * Marshal.SizeOf<VaSpaceLocationExt>());
            }
            finally
            {
                Marshal.FreeHGlobal(spacesBuffer);
                Marshal.FreeHGlobal(locationsBuffer);
            }
        }

        internal static SpaceLocation ReadSpaceLocation(IntPtr ptr)
        {
            VaSpaceLocationExt? data = Marshal.PtrToStructure<VaSpaceLocationExt>(ptr);
            if (data is null)
            {
                return new();
            }
            else
            {
                return new SpaceLocation
                {
                    pose = data.Value.pose,
                    isTracked = data.Value.isTracked != 0
                };
            }
        }
    }
}
