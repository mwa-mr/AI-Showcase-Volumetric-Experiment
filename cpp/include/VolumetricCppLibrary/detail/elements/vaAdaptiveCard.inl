#pragma once

#include <functional>

namespace va {
    inline void AdaptiveCardElement::SetTemplate(const std::string& jsonTemplate) {
        CHECK_VA(Context().pfn.vaSetElementPropertyString(m_handle, VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_TEMPLATE_EXT, jsonTemplate.c_str()));
    }
    inline void AdaptiveCardElement::SetData(const std::string& jsonData) {
        CHECK_VA(Context().pfn.vaSetElementPropertyString(m_handle, VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_DATA_EXT, jsonData.c_str()));
    }

    inline void AdaptiveCardElement::PollAdaptiveCardActionInvokedData() {
        if (onAction) {
            VaAdaptiveCardActionInvokedDataExt actionData = {VA_TYPE_ADAPTIVE_CARD_ACTION_INVOKED_DATA_EXT};

            CHECK_VA(Context().pfn.vaGetNextAdaptiveCardActionInvokedDataExt(ElementHandle(), &actionData));

            // c++ 14 doesn't support non-const std::string::data() function.
            const auto get_string_buffer = [](std::string& s) -> char* { return const_cast<char*>(s.data()); };

            while (actionData.verbCountOutput > 0 || actionData.dataCountOutput > 0) {
                // For the allocations, we need to reserve enough space to contain the whole string, including the null terminator. That is
                // *CountOutput. But our local string length needs to be sized to a length not including the null terminator. Resize here
                // doesn't affect the amount of data, just the view of the data.
                std::string verb;
                verb.reserve(actionData.verbCountOutput);
                verb.resize(actionData.verbCountOutput - 1);
                actionData.verbCapacityInput = static_cast<uint32_t>(verb.capacity());
                actionData.verb = get_string_buffer(verb);

                std::string data;
                data.reserve(actionData.dataCountOutput);
                data.resize(actionData.dataCountOutput - 1);
                actionData.dataCapacityInput = static_cast<uint32_t>(data.capacity());
                actionData.data = get_string_buffer(data);

                CHECK_VA(Context().pfn.vaGetNextAdaptiveCardActionInvokedDataExt(ElementHandle(), &actionData));

                onAction(verb, data);

                // Now see if there are any more actions.  Exit loop if output is empty.
                actionData = {VA_TYPE_ADAPTIVE_CARD_ACTION_INVOKED_DATA_EXT};
                CHECK_VA(Context().pfn.vaGetNextAdaptiveCardActionInvokedDataExt(ElementHandle(), &actionData));
            }
        }
    }
} // namespace va
