#pragma once
#include <vector>
#include <string>
#include <volumetric/volumetric.h>
#include <vaCtor.h>

namespace va {

    class Extensions : NonMovable {
    public:
        // Example: for an extension name `VA_EXT_extension_name`, there will be two fields define for it.
        //  bool   VA_EXT_extension_name_enabled; // it will be true if the extension is enabled by the current instance
        //  uint32 VA_EXT_extension_name_version; // it will be > 0 if the extension is enabled by the current instance
#define DEFINE_EXTENSION_FIELD(name, _) \
    bool name##_enabled = false;        \
    uint32_t name##_version = 0;

        LIST_EXTENSIONS(DEFINE_EXTENSION_FIELD)

#undef DEFINE_EXTENSION_FIELD

        const std::vector<const char*> EnabledExtensions;
        const std::vector<const char*> MissingRequiredExtensions;

    public:
        Extensions(const std::vector<VaExtensionProperties>& supportedExtensions,
                   const std::vector<std::string>& requiredExtensions,
                   const std::vector<std::string>& optionalExtensions)
            : EnabledExtensions(ComputeEnabledExtensions(supportedExtensions, requiredExtensions, optionalExtensions))
            , MissingRequiredExtensions(ComputeMissingRequiredExtensions(requiredExtensions)) {}

    private:
        std::vector<const char*> ComputeEnabledExtensions(const std::vector<VaExtensionProperties>& supportedExtensions,
                                                          const std::vector<std::string>& requiredExtensions,
                                                          const std::vector<std::string>& optionalExtensions) {
            std::vector<const VaExtensionProperties*> enabledExtensionProperties;
            for (const VaExtensionProperties& supportedExtension : supportedExtensions) {
                for (const auto& requiredExtension : requiredExtensions) {
                    if (requiredExtension == supportedExtension.extensionName) {
                        enabledExtensionProperties.push_back(&supportedExtension);
                        break;
                    }
                }
                for (const auto& optionalExtension : optionalExtensions) {
                    if (optionalExtension == supportedExtension.extensionName) {
                        enabledExtensionProperties.push_back(&supportedExtension);
                        break;
                    }
                }
            }

#define SET_EXTENSION_IF_MATCH(name, _)      \
    if (strcmp(extensionName, #name) == 0) { \
        name##_enabled = true;               \
        name##_version = extensionVersion;   \
        continue;                            \
    }

            std::vector<const char*> enabledExtensions;
            for (const VaExtensionProperties* enabledExtensionProperty : enabledExtensionProperties) {
                const char* extensionName = enabledExtensionProperty->extensionName;
                const uint32_t extensionVersion = enabledExtensionProperty->extensionVersion;
                enabledExtensions.push_back(extensionName);

                LIST_EXTENSIONS(SET_EXTENSION_IF_MATCH)
            }

#undef SET_EXTENSION_IF_MATCH

            return enabledExtensions;
        }

        std::vector<const char*> ComputeMissingRequiredExtensions(const std::vector<std::string>& requiredExtensions) {
            std::vector<const char*> missingRequiredExtensions;

            for (const auto& requiredExtension : requiredExtensions) {
                auto it = std::find_if(
                    EnabledExtensions.begin(), EnabledExtensions.end(), [&](const char* enabledExtension) { return strcmp(enabledExtension, requiredExtension.c_str()) == 0; });

                if (it == EnabledExtensions.end()) {
                    missingRequiredExtensions.push_back(requiredExtension.c_str());
                }
            }

            return missingRequiredExtensions;
        }
    };

} // namespace va
