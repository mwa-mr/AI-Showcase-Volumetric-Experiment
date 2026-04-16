#pragma once

namespace va {
    inline HandTracker::HandTracker(va::Volume& volume)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {}

    inline void HandTracker::Update() {
        VaJointLocateInfoExt locateInfo{VA_TYPE_JOINT_LOCATE_INFO_EXT};
        locateInfo.baseSpace = VA_SPACE_TYPE_VOLUME_CONTENT_EXT;
        locateInfo.jointSet = VA_JOINT_SET_HAND_EXT;

        VaJointLocationsExt locations{VA_TYPE_JOINT_LOCATIONS_EXT};
        locations.jointCount = VA_HAND_JOINT_COUNT_EXT;

        for (side_t side : {left_side, right_side}) {
            auto& result = m_jointLocations[side];

            locateInfo.side = detail::Convert(side);
            locations.jointPoses = result.poses;
            locations.jointRadii = result.radii;
            CHECK_VA(Context().pfn.vaLocateJointsExt(ElementHandle(), &locateInfo, &locations));
            result.hasDataSource = locations.hasDataSource;
            result.isTracked = locations.isTracked;
        }
    }

    inline const va::JointLocations& HandTracker::JointLocations(side_t side) const {
        return m_jointLocations[side];
    }

    inline SpaceLocator::SpaceLocator(va::Volume& volume)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {}

    inline void SpaceLocator::Update() {
        VaSpaceLocateInfoExt locateInfo{VA_TYPE_SPACE_LOCATE_INFO_EXT};
        locateInfo.baseSpace = VA_SPACE_TYPE_VOLUME_CONTENT_EXT;
        locateInfo.spaceCount = sizeof(m_spaces) / sizeof(m_spaces[0]);
        locateInfo.spaces = m_spaces;

        static_assert(sizeof(m_spaces) / sizeof(VaSpaceTypeExt) == sizeof(m_spaceLocations) / sizeof(VaSpaceLocationExt),
                      "m_spaces and m_spaceLocations must have the same number of elements for two call idiom.");

        VaSpaceLocationsExt locations{VA_TYPE_SPACE_LOCATIONS_EXT};
        locations.locationCount = sizeof(m_spaceLocations) / sizeof(VaSpaceLocationExt);
        locations.locations = reinterpret_cast<VaSpaceLocationExt*>(&m_spaceLocations);

        CHECK_VA(Context().pfn.vaLocateSpacesExt(ElementHandle(), &locateInfo, &locations));
    }

    inline const SpaceLocations& SpaceLocator::Locations() const {
        return m_spaceLocations;
    }

} // namespace va
