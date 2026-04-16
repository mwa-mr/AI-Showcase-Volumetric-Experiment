// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Api = Detail.Api;

    /// <summary>
    /// JointLocations represents the locations of hand joints in a hand tracker.
    /// It contains the poses and radii of each joint, as well as flags indicating whether the hand is tracked
    /// </summary>
    public class JointLocations
    {
        internal readonly VaPosef[] _jointPoses = new VaPosef[Api.VA_HAND_JOINT_COUNT_EXT];
        internal readonly float[] _jointRadii = new float[Api.VA_HAND_JOINT_COUNT_EXT];

        /// <summary>
        /// Gets whether the hand tracker has a valid data source.
        /// When false, there is no hand tracker device available and the DataSource, IsTracked,
        /// time, and joint data reflect previously cached values.
        /// </summary>
        public bool HasDataSource { get; internal set; }

        /// <summary>
        /// Gets the data source type for hand tracking.
        /// When HasDataSource is false, this is set to Unavailable (volume may not be interactive).
        /// </summary>
        public VaHandTrackingDataSourceExt DataSource { get; internal set; }

        /// <summary>
        /// Gets whether the hand is currently tracked by the hand tracker.
        /// When false, the pose and radius data for the joints may not be actively tracked
        /// but the data is still available if HasDataSource is true, and the data 
        /// might be infferred from previous tracking data.
        /// </summary>
        public bool IsTracked { get; internal set; }

        /// <summary>
        /// Gets the pose of the joint at the specified index.
        /// The index corresponds to the joint type defined in VaHandJointExt enum.
        /// </summary>
        public ref VaPosef Pose(int joint) => ref _jointPoses[joint];

        /// <summary>
        /// Gets the pose of the joint at the specified joint type.
        /// The joint type is defined in VaHandJointExt enum.
        /// </summary>
        public ref VaPosef Pose(VaHandJointExt joint) => ref _jointPoses[(int)joint];


        /// <summary>
        /// Gets the radius of the joint at the specified index.
        /// The index corresponds to the joint type defined in VaHandJointExt enum.
        /// </summary>
        public float Radius(int joint) => _jointRadii[joint];

        /// <summary>
        /// Gets the radius of the joint at the specified joint type.
        /// The joint type is defined in VaHandJointExt enum.
        /// </summary>
        public float Radius(VaHandJointExt joint) => _jointRadii[(int)joint];
    }

    /// <summary>
    /// HandTracker represents a hand tracking device that can track the positions of hand joints.
    /// It provides access to the joint locations for both left and right hands.
    /// </summary>
    public class HandTracker : Element
    {
        /// <summary>
        /// The number of joints in a hand tracker for each hand.
        /// </summary>
        public const uint JointCount = Api.VA_HAND_JOINT_COUNT_EXT;

        /// <summary>
        /// Creates a new hand tracker in the specified volume.
        /// </summary>
        public HandTracker(Volume volume)
            : base(VaElementType.HandTrackerExt, volume, CreateElement)
        {
        }

        private readonly JointLocations[] _data = new JointLocations[2] {
            new JointLocations(), // Left hand
            new JointLocations()  // Right hand
        };


        // <summary>
        // Get the latest joint locations of left (index 0) or right (index 1) hand.
        // </summary>
        // <remarks>
        // The joint locations are only valid if there is a valid data source and the hand is tracked,
        // and the user puts the volume in the interactive mode.
        // Note that a volume by default disallows the interactive mode, unless the app explicitly allows it
        // by calling Volume.Container.AllowInteractiveMode(true).
        // </remarks>
        public IReadOnlyList<JointLocations> JointLocations => _data;

        /// <summary>
        /// Updates the joint locations for both left and right hands.
        /// Application typically calls this method on each update to refresh the hand tracking data.
        /// </summary>
        public void Update()
        {
            Api.VaJointLocateInfoExt locateInfo = new();
            locateInfo.type = Api.VaStructureType.VA_TYPE_JOINT_LOCATE_INFO_EXT;
            locateInfo.baseSpace = Api.VaSpaceTypeExt.VA_SPACE_TYPE_VOLUME_CONTENT_EXT;
            locateInfo.jointSet = Api.VaJointSetExt.VA_JOINT_SET_HAND_EXT;

            Api.VaJointLocationsExt locations = new();
            locations.type = Api.VaStructureType.VA_TYPE_JOINT_LOCATIONS_EXT;
            locations.jointCount = Api.VA_HAND_JOINT_COUNT_EXT;

            IntPtr posesBuffer = Marshal.AllocHGlobal(Marshal.SizeOf<VaPosef>() * (int)Api.VA_HAND_JOINT_COUNT_EXT);
            IntPtr radiiBuffer = Marshal.AllocHGlobal(sizeof(float) * (int)Api.VA_HAND_JOINT_COUNT_EXT);
            try
            {
                foreach (int side in new[] { 0, 1 })
                {
                    locateInfo.side = side == 0 ? Api.VaSideExt.VA_SIDE_LEFT_EXT : Api.VaSideExt.VA_SIDE_RIGHT_EXT;
                    locations.jointPoses = posesBuffer;
                    locations.jointRadii = radiiBuffer;

                    Api.CheckResult(Api.vaLocateJointsExt(Handle, locateInfo, out locations));

                    _data[side].HasDataSource = locations.hasDataSource != 0;
                    _data[side].DataSource = (VaHandTrackingDataSourceExt)locations.dataSource;
                    _data[side].IsTracked = locations.isTracked != 0;
                    for (int i = 0; i < Api.VA_HAND_JOINT_COUNT_EXT; i++)
                    {
                        _data[side]._jointPoses[i] = Marshal.PtrToStructure<VaPosef>(posesBuffer + i * Marshal.SizeOf<VaPosef>());

                        int raw = Marshal.ReadInt32(radiiBuffer, i * sizeof(float));
                        _data[side]._jointRadii[i] = BitConverter.ToSingle(BitConverter.GetBytes(raw), 0);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(posesBuffer);
                Marshal.FreeHGlobal(radiiBuffer);
            }
        }
    }
}
