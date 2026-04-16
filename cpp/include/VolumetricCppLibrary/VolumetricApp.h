#pragma once

#include <cassert>
#include <chrono>
#include <cmath>
#include <future>
#include <memory>
#include <string>
#include <thread>
#include <vector>

#if _HAS_CXX20
#include <span>
#endif

#include <volumetric/volumetric.h>
#include <volumetric/volumetric_reflection.h>

#include <vaCtor.h>
#include <vaEnums.h>
#include <vaError.h>
#include <vaExtensions.h>
#include <vaFlags.h>
#include <vaFunctions.h>
#include <vaMath.h>
#include <vaString.h>
#include <vaVersion.h>

#if defined(_WIN32)
#include <vaWindows.h>
#endif // _WIN32

#include <api/vaApp.h>
#include <api/vaVolume.h>
#include <api/vaInput.h>
#include <api/vaElement.h>

#include <detail/components/vaAppContext.h>
#include <detail/components/vaElementList.h>

#include <detail/elements/vaAdaptiveCard.inl>
#include <detail/elements/vaElement.inl>
#include <detail/elements/vaInput.inl>
#include <detail/elements/vaMaterialResource.inl>
#include <detail/elements/vaMeshResource.inl>
#include <detail/elements/vaModelResource.inl>
#include <detail/elements/vaTextureResource.inl>
#include <detail/elements/vaVisualElement.inl>

#include <detail/vaApp.inl>
#include <detail/vaSession.h>
#include <detail/vaSession.inl>
#include <detail/vaVolume.inl>
