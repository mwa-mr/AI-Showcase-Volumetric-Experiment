#pragma once

#include <volumetric/volumetric.h>

namespace va {
    namespace detail {
        template <typename T>
        struct is_bitmask_enum : std::false_type {};

#define ENABLE_BITMASK_OPERATORS(x) \
    template <>                     \
    struct detail::is_bitmask_enum<x> : std::true_type {}

        template <typename E>
        constexpr std::enable_if_t<is_bitmask_enum<E>::value, E> operator|(E lhs, E rhs) {
            using U = std::underlying_type_t<E>;
            return static_cast<E>(static_cast<U>(lhs) | static_cast<U>(rhs));
        }

        template <typename E>
        constexpr std::enable_if_t<is_bitmask_enum<E>::value, E> operator&(E lhs, E rhs) {
            using U = std::underlying_type_t<E>;
            return static_cast<E>(static_cast<U>(lhs) & static_cast<U>(rhs));
        }

        template <typename E>
        constexpr std::enable_if_t<is_bitmask_enum<E>::value, E> operator^(E lhs, E rhs) {
            using U = std::underlying_type_t<E>;
            return static_cast<E>(static_cast<U>(lhs) ^ static_cast<U>(rhs));
        }

        template <typename E>
        constexpr std::enable_if_t<is_bitmask_enum<E>::value, E> operator~(E val) {
            using U = std::underlying_type_t<E>;
            return static_cast<E>(~static_cast<U>(val));
        }

        template <typename E>
        std::enable_if_t<is_bitmask_enum<E>::value, E&> operator|=(E& lhs, E rhs) {
            lhs = lhs | rhs;
            return lhs;
        }

        template <typename E>
        std::enable_if_t<is_bitmask_enum<E>::value, E&> operator&=(E& lhs, E rhs) {
            lhs = lhs & rhs;
            return lhs;
        }
    } // namespace detail

    using detail::operator|;
    using detail::operator&;
    using detail::operator^;
    using detail::operator~;
    using detail::operator|=;
    using detail::operator&=;

    ENABLE_BITMASK_OPERATORS(VaVolumeRotationLockFlags);
    ENABLE_BITMASK_OPERATORS(VaVolumeContainerModeFlagsExt);
} // namespace va
