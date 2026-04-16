#pragma once

#include <string>
#include <volumetric/volumetric.h>
#include <volumetric/volumetric_reflection.h>

namespace va {

#define IF_MATCH_STR(name, val)                                  \
    if (static_cast<uint32_t>(name) == static_cast<uint32_t>(e)) \
        return #name;

// Returns C string pointing to a string literal. Unknown values are returned as 'Unknown <type>'.
#define MAKE_TO_STRING_FUNC(enumType)                                 \
    constexpr const char* ToString(enumType e) noexcept {             \
        LIST_ENUM_##enumType(IF_MATCH_STR);                           \
        return e ? "Unknown " #enumType##" Value" : #enumType##"(0)"; \
    }

    LIST_ENUM_TYPES(MAKE_TO_STRING_FUNC);

#undef IF_MATCH_STR
#undef MAKE_TO_STRING_FUNC

#define CHECK_ENUM_SIZE(enumType) static_assert(sizeof(enumType) == sizeof(int32_t), "sizeof(" #enumType ") should be 32 bits.");
    LIST_ENUM_TYPES(CHECK_ENUM_SIZE)
#undef CHECK_ENUM_SIZE

} // namespace va
