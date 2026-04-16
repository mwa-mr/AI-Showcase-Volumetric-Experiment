#pragma once

#include <volumetric/volumetric.h>

// Major version is 0 means it's at preview phase. Breaking change may happen on minor version changes.
constexpr bool VA_IS_PREVIEW_VERSION = VA_VERSION_MAJOR(VA_CURRENT_API_VERSION) == 0;

// Typically a volumetric app is compatible with all future runtime releases with the same major version.
constexpr VaVersion VA_MAXIMUM_VERSION = VA_IS_PREVIEW_VERSION
                                           ? VA_MAKE_VERSION(VA_VERSION_MAJOR(VA_CURRENT_API_VERSION), VA_VERSION_MINOR(VA_CURRENT_API_VERSION), VA_VERSION_PATCH(UINT64_MAX))
                                           : VA_MAKE_VERSION(VA_VERSION_MAJOR(VA_CURRENT_API_VERSION), VA_VERSION_MINOR(UINT64_MAX), VA_VERSION_PATCH(UINT64_MAX));

static_assert(VA_VERSION_MAJOR(VA_MAXIMUM_VERSION) == VA_VERSION_MAJOR(VA_CURRENT_API_VERSION));
static_assert(VA_VERSION_MINOR(VA_MAXIMUM_VERSION) == (VA_IS_PREVIEW_VERSION ? VA_VERSION_MINOR(VA_CURRENT_API_VERSION) : 0xffffULL));
static_assert(VA_VERSION_PATCH(VA_MAXIMUM_VERSION) == 0xffffffffULL);
