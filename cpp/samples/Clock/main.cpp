#include <chrono>
#include <format>

#include <vaError.h>
#include <vaMath.h>
#include <vaUuid.h>
#include <VolumetricApp.h>

using namespace std::chrono;
using namespace std::chrono_literals;

namespace {
    constexpr char s_AdaptiveCardTemplate[] = R"(
    {
        "type": "AdaptiveCard",
        "body": [
            {
                "id": "time",
                "type": "TextBlock",
                "text": "${currentTime}",
                "horizontalAlignment": "center"
            },
            {
                "id": "timezone",
                "type": "TextBlock",
                "text": "${timezone}",
                "horizontalAlignment": "center"
            }
        ],
        "actions": [
            {
                "type": "Action.Execute",
                "title": "Timezone -",
                "verb": "dec"
            },
            {
                "type": "Action.Execute",
                "title": "Timezone +",
                "verb": "inc"
            }
        ],
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.4"
    })";

    std::string FormatAdaptiveCardData(auto time) {
        // For std::format, any json { and } need to be escaped with {{ and }}.
        constexpr const char* s_AdaptiveCardDataFormat = R"(
        {{
            "currentTime": "{0:%a %D} {1:%T}",
            "timezone": "(UTC{0:%Ez}) {2}"
        }})";

        const auto local = time.get_local_time();
        hh_mm_ss time_of_day{floor<seconds>(local - floor<days>(local))};

        return std::format(s_AdaptiveCardDataFormat, time, time_of_day, time.get_time_zone()->name());
    }

    std::vector<const std::chrono::time_zone*> SelectTimeZones() {
        const std::chrono::tzdb& timezones = std::chrono::get_tzdb();
        std::vector<std::string> selected_zones = {
            "Pacific/Honolulu",     // UTC-10:00
            "America/Anchorage",    // UTC-09:00
            "America/Los_Angeles",  // UTC-08:00
            "America/Denver",       // UTC-07:00
            "America/Chicago",      // UTC-06:00
            "America/New_York",     // UTC-05:00
            "America/Santiago",     // UTC-04:00
            "America/Buenos_Aires", // UTC-03:00
            "Europe/London",        // UTC±00:00
            "Europe/Berlin",        // UTC+01:00
            "Europe/Athens",        // UTC+02:00
            "Asia/Riyadh",          // UTC+03:00
            "Asia/Dubai",           // UTC+04:00
            "Asia/Karachi",         // UTC+05:00
            "Asia/Dhaka",           // UTC+06:00
            "Asia/Bangkok",         // UTC+07:00
            "Asia/Shanghai",        // UTC+08:00
            "Asia/Tokyo",           // UTC+09:00
            "Australia/Sydney",     // UTC+10:00
            "Pacific/Auckland"      // UTC+12:00
        };
        std::vector<const std::chrono::time_zone*> result;
        for (const auto& zone : selected_zones) {
            if (auto tz = timezones.locate_zone(zone)) {
                result.push_back(tz);
            }
        }
        return result;
    }

    bool SameTimeZone(const std::chrono::time_zone* lhs, const std::chrono::time_zone* rhs) {
        auto now = std::chrono::system_clock::now();
        auto left_time = std::chrono::zoned_time{lhs, now};
        auto right_time = std::chrono::zoned_time{rhs, now};
        return left_time.get_info().offset == right_time.get_info().offset;
    }

    class ClockVolume : public va::Volume {
    public:
        explicit ClockVolume(va::VolumetricApp& app)
            : va::Volume(app)
            , m_timezone(std::chrono::current_zone())
            , m_timezones(SelectTimeZones()) {
            InitializeVolume();
        }

        ClockVolume(va::VolumetricApp& app, VaUuid volumeRestoreId)
            : va::Volume(app, volumeRestoreId)
            , m_timezone(std::chrono::current_zone())
            , m_timezones(SelectTimeZones()) {
            InitializeVolume();
            RestoreClockState();
        }

    private:
        void InitializeVolume() {
            Container().SetDisplayName("Volumetric Clock");
            Container().SetRotationLock((VaVolumeRotationLockFlags)(VA_VOLUME_ROTATION_LOCK_X | VA_VOLUME_ROTATION_LOCK_Z));
            Container().SetThumbnailIconUri(va::windows::GetLocalAssetUri("ClockThumbnail.png").c_str());
            Container().SetThumbnailModelUri(va::windows::GetLocalAssetUri("ClockThumbnail.glb").c_str());

            onReady = std::bind(&ClockVolume::InitializeClock, this);
            onUpdate = std::bind(&ClockVolume::UpdateClock, this);
            onClose = std::bind(&ClockVolume::OnClose, this);

            // Locate the index for the current timezone.
            while (!SameTimeZone(m_timezones[m_tzdbIndex], m_timezone) && ++m_tzdbIndex)
                ;
        }

        void InitializeClock() {
            m_elements.adaptiveCard = CreateElement<va::AdaptiveCardElement>();
            m_elements.adaptiveCard->SetTemplate(s_AdaptiveCardTemplate);
            m_elements.adaptiveCard->SetData(FormatAdaptiveCardData(GetTime()));
            m_elements.adaptiveCard->onAction = std::bind(&ClockVolume::OnAdaptiveCardAction, this, std::placeholders::_1, std::placeholders::_2);

            m_elements.model = CreateElement<va::ModelResource>();
            m_elements.clock = CreateElement<va::VisualElement>(*m_elements.model);
            m_elements.hourHand = CreateElement<va::VisualElement>(*m_elements.clock, "def_hours");
            m_elements.minuteHand = CreateElement<va::VisualElement>(*m_elements.clock, "def_minutes");
            m_elements.secondHand = CreateElement<va::VisualElement>(*m_elements.clock, "def_seconds");

            m_elements.model->SetModelUri(va::windows::GetLocalAssetUri("clock.glb"));

            UpdateClock(); // Update the clock hands and adaptive card data.
        }

        void UpdateClock() {
            constexpr float XRot = va::degrees_to_radians(90.f);

            if (m_elements.model->IsReady()) {
                const auto time = GetTime();
                const auto local = time.get_local_time();

                hh_mm_ss time_of_day{floor<milliseconds>(local - floor<days>(local))};

                const float hourAngle = fmodf(((time_of_day.hours().count() * 60 + time_of_day.minutes().count()) * 0.5f), 360.f);
                const float minAngle = fmodf((time_of_day.minutes().count() * 60 + time_of_day.seconds().count()) * 0.1f, 360.f);
                const float secAngle = (time_of_day.seconds().count() * 1000 + time_of_day.subseconds().count()) * 0.006f;

                // The rotation must be consistent with the corresponding gltf node local transform in Glb file.
                // In this case, the clock.glb file contains "def_hours", "def_minutes" and "def_seconds" nodes,
                // and they are rotated 90 degrees around the X axis, and clockwise rotation is around Y axis.
                // One can inspect the gltf node transforms in a gltf viewer, such as https://sandbox.babylonjs.com/
                const auto hourQuat = va::quaternion::from_eular_angles({XRot, va::degrees_to_radians(hourAngle), 0});
                const auto minsQuat = va::quaternion::from_eular_angles({XRot, va::degrees_to_radians(minAngle), 0});
                const auto secQuat = va::quaternion::from_eular_angles({XRot, va::degrees_to_radians(secAngle), 0});

                m_elements.hourHand->SetOrientation(hourQuat);
                m_elements.minuteHand->SetOrientation(minsQuat);
                m_elements.secondHand->SetOrientation(secQuat);
                m_elements.adaptiveCard->SetData(FormatAdaptiveCardData(time));
            }

            RequestUpdateAfter(1s); // Request to update the clock hands and adaptive card data every second.
        }

        void OnAdaptiveCardAction(const std::string& verb, const std::string& /*data*/) {
            if (verb == "inc") {
                m_tzdbIndex++;
                if (m_tzdbIndex >= m_timezones.size()) {
                    m_tzdbIndex = 0;
                }
            } else if (verb == "dec") {
                if (m_tzdbIndex == 0) {
                    m_tzdbIndex = m_timezones.size() - 1;
                } else {
                    m_tzdbIndex--;
                }
            }

            m_timezone = m_timezones[m_tzdbIndex];

            // Update the adaptive card for the timezone change.
            RequestUpdate();
        }

        void RestoreClockState() {
            VaUuid restoreId = GetRestoreId();
            int index;
            if (va::windows::TryReadSettingAsInt(va::to_wstring(restoreId).c_str(), L"TimezoneIndex", index)) {
                m_tzdbIndex = std::clamp(index, 0, (int)m_timezones.size() - 1);
                m_timezone = m_timezones[m_tzdbIndex];
            }
        }

        void OnClose() {
            m_elements = {};
            App().RequestExit();
        }

        auto GetTime() const {
            auto now = std::chrono::system_clock::now();
            auto now_seconds = std::chrono::time_point_cast<std::chrono::milliseconds>(now);
            return std::chrono::zoned_time{m_timezone, now_seconds};
        }

        const std::chrono::time_zone* m_timezone;
        const std::vector<const std::chrono::time_zone*> m_timezones;
        size_t m_tzdbIndex = 0;

        struct {
            va::ModelResource* model{};
            va::VisualElement* clock{};
            va::VisualElement* hourHand{};
            va::VisualElement* minuteHand{};
            va::VisualElement* secondHand{};
            va::AdaptiveCardElement* adaptiveCard{};
        } m_elements = {};
    };
} // namespace

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int) {
    va::AppCreateInfo createInfo{};
    createInfo.applicationName = "cpp_clock";
    createInfo.requiredExtensions = {VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                     VA_EXT_ADAPTIVE_CARD_ELEMENT_EXTENSION_NAME,
                                     VA_EXT_VOLUME_CONTAINER_MODES_EXTENSION_NAME,
                                     VA_EXT_VOLUME_CONTAINER_THUMBNAIL_EXTENSION_NAME};
    // TODO: The clock should probably be VA_VOLUME_RESTORE_BEHAVIOR_BY_APP_EXT instead.
    createInfo.volumeRestoreBehavior = VA_VOLUME_RESTORE_BEHAVIOR_NO_RESTORE_EXT;

    // Create a volumetric app with above information.
    auto app = va::CreateVolumetricApp(std::move(createInfo));

    // Create a new clock volume when the app starts.
    app->onStart = [](auto& app) { app.CreateVolume<ClockVolume>(); };

    // Restore the clock volume when the app is relaunched.
    app->onRestoreVolumeRequest = [](auto& app, const VaUuid& restoreId) { app.CreateVolume<ClockVolume>(restoreId); };

    // Run the app loop until app exits.
    return app->Run();
}
