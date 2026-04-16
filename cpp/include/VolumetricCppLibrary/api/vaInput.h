#pragma once

#include "vaElement.h"

namespace va {

    typedef size_t side_t;
    constexpr side_t left_side = 0;
    constexpr side_t right_side = 1;
    constexpr side_t side_count = 2;

    namespace detail {
        static_assert(left_side == 0 && right_side == 1, "side_t values must be 0 and 1");
        constexpr side_t Convert(VaSideExt side) {
            switch (side) {
            default:
            case VA_SIDE_LEFT_EXT:
                return left_side;
            case VA_SIDE_RIGHT_EXT:
                return right_side;
            }
        }

        constexpr VaSideExt Convert(side_t side) {
            switch (side) {
            default:
            case left_side:
                return VA_SIDE_LEFT_EXT;
            case right_side:
                return VA_SIDE_RIGHT_EXT;
            }
        }

    } // namespace detail

    struct SpaceLocations {
        VaSpaceLocationExt volumeContainer{};
        VaSpaceLocationExt volumeContent{};
        VaSpaceLocationExt viewer{};
        VaSpaceLocationExt local{};
    };

    struct JointLocations {
        bool hasDataSource{};
        bool isTracked{};
        VaPosef poses[VA_HAND_JOINT_COUNT_EXT]{};
        float radii[VA_HAND_JOINT_COUNT_EXT]{};
    };

    struct HandTracker : ElementOfType<VA_ELEMENT_TYPE_HAND_TRACKER_EXT> {
        HandTracker(va::Volume& volume);
        void Update();

        // <summary>
        // Get the joint locations from the hand tracker.
        // </summary>
        // <remarks>
        // The joint locations are only valid if there is a valid data source and the hand is tracked,
        // and the user puts the volume in the interactive mode.
        // Note that a volume by default disallows the interactive mode, unless the app explicitly allows it
        // by calling Volume.Container.AllowInteractiveMode(true).
        // </remarks>
        const va::JointLocations& JointLocations(side_t side) const;

    private:
        va::JointLocations m_jointLocations[side_count]{};
    };

    struct SpaceLocator : ElementOfType<VA_ELEMENT_TYPE_SPACE_LOCATOR_EXT> {
        SpaceLocator(va::Volume& volume);
        void Update();

        const SpaceLocations& Locations() const;

    private:
        constexpr static VaSpaceTypeExt m_spaces[] = {
            VA_SPACE_TYPE_VOLUME_CONTAINER_EXT,
            VA_SPACE_TYPE_VOLUME_CONTENT_EXT,
            VA_SPACE_TYPE_VIEWER_EXT,
            VA_SPACE_TYPE_LOCAL_EXT,
        };
        SpaceLocations m_spaceLocations{};
    };

} // namespace va
