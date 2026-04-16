#pragma once

#include <cmath>
#include <volumetric/volumetric.h>
#include <DirectXMath.h>

namespace va {
    constexpr float PI = DirectX::XM_PI;
    constexpr float degrees_to_radians(float degrees) noexcept {
        return degrees * PI / 180.0f;
    }

    namespace quaternion {
        constexpr VaQuaternionf identity = {0, 0, 0, 1};

        inline float length(const VaQuaternionf& quaternion) noexcept;
        inline bool is_normalized(const VaQuaternionf& quaternion) noexcept;
        inline VaQuaternionf from_axis_rotation(const VaVector3f& axis, float angleInRadians) noexcept;
        inline VaQuaternionf from_eular_angles(const VaVector3f& anglesInRadians) noexcept;
        inline VaQuaternionf slerp(const VaQuaternionf& a, const VaQuaternionf& b, float alpha) noexcept;
    } // namespace quaternion

    namespace vector {
        constexpr VaVector3f zero = {0, 0, 0};
        constexpr VaVector3f one = {1, 1, 1};

        constexpr VaVector3f up = {0, 1, 0};
        constexpr VaVector3f down = {0, -1, 0};
        constexpr VaVector3f right = {1, 0, 0};
        constexpr VaVector3f left = {-1, 0, 0};
        constexpr VaVector3f backward = {0, 0, 1};
        constexpr VaVector3f forward = {0, 0, -1};

        constexpr float dot(const VaVector3f& a, const VaVector3f& b) noexcept;
        inline float length(const VaVector3f& v) noexcept;
        inline VaVector3f normalize(const VaVector3f& a) noexcept;
    } // namespace vector

    namespace pose {
        constexpr VaPosef identity = {quaternion::identity, vector::zero};

        // Returns aInC = aInB * bInC
        VaPosef Combine(const VaPosef& aInB, const VaPosef& bInC);

        // Returns bInA = aInB^-1
        VaPosef Invert(const VaPosef& aInB);
    } // namespace pose

    namespace size {
        constexpr VaExtent3Df zero = {0, 0, 0};
        constexpr VaExtent3Df one = {1, 1, 1};
    } // namespace size

    namespace math {
        namespace detail {
            template <typename X, typename Y>
            constexpr const X& implement_math_cast(const Y& value) noexcept {
                static_assert(std::is_trivially_copyable<X>::value, "Unsafe to cast between non-POD types.");
                static_assert(std::is_trivially_copyable<Y>::value, "Unsafe to cast between non-POD types.");
                static_assert(!std::is_pointer<X>::value, "Incorrect cast between pointer types.");
                static_assert(!std::is_pointer<Y>::value, "Incorrect cast between pointer types.");
                static_assert(sizeof(X) == sizeof(Y), "Incorrect cast between types with different sizes.");
                return reinterpret_cast<const X&>(value);
            }
        } // namespace detail

#define DEFINE_CAST_FROM(X, Y)                         \
    constexpr const X& cast(const Y& value) noexcept { \
        return detail::implement_math_cast<X>(value);  \
    }
        DEFINE_CAST_FROM(DirectX::XMFLOAT3, VaVector3f);
        DEFINE_CAST_FROM(VaVector3f, DirectX::XMFLOAT3);
        DEFINE_CAST_FROM(DirectX::XMFLOAT4, VaQuaternionf);
        DEFINE_CAST_FROM(VaQuaternionf, DirectX::XMFLOAT4);
#undef DEFINE_CAST

    } // namespace math

} // namespace va

#pragma region Implementation

namespace va {
    namespace math {
        namespace detail {
            // Convert VA types to DX
            inline DirectX::XMVECTOR XM_CALLCONV LoadVaVector3(const VaVector3f& vector) noexcept {
                return DirectX::XMLoadFloat3(&va::math::cast(vector));
            }

            inline DirectX::XMVECTOR XM_CALLCONV LoadVaQuaternion(const VaQuaternionf& quaternion) noexcept {
                return DirectX::XMLoadFloat4(&va::math::cast(quaternion));
            }

            // Convert DX types to VA
            inline void XM_CALLCONV StoreVaVector3(VaVector3f* outVec, DirectX::FXMVECTOR inVec) noexcept {
                DirectX::XMStoreFloat3(const_cast<DirectX::XMFLOAT3*>(&va::math::detail::implement_math_cast<DirectX::XMFLOAT3>(*outVec)), inVec);
            }

            inline void XM_CALLCONV StoreVaQuaternion(VaQuaternionf* outQuat, DirectX::FXMVECTOR inQuat) noexcept {
                DirectX::XMStoreFloat4(const_cast<DirectX::XMFLOAT4*>(&va::math::detail::implement_math_cast<DirectX::XMFLOAT4>(*outQuat)), inQuat);
            }

            static_assert(offsetof(DirectX::XMFLOAT3, x) == offsetof(VaVector3f, x));
            static_assert(offsetof(DirectX::XMFLOAT3, y) == offsetof(VaVector3f, y));
            static_assert(offsetof(DirectX::XMFLOAT3, z) == offsetof(VaVector3f, z));

            static_assert(offsetof(DirectX::XMFLOAT4, x) == offsetof(VaQuaternionf, x));
            static_assert(offsetof(DirectX::XMFLOAT4, y) == offsetof(VaQuaternionf, y));
            static_assert(offsetof(DirectX::XMFLOAT4, z) == offsetof(VaQuaternionf, z));
            static_assert(offsetof(DirectX::XMFLOAT4, w) == offsetof(VaQuaternionf, w));
        } // namespace detail
    } // namespace math

    namespace quaternion {
        inline float length(const VaQuaternionf& quaternion) noexcept {
            DirectX::XMVECTOR vector = va::math::detail::LoadVaQuaternion(quaternion);
            return DirectX::XMVectorGetX(DirectX::XMVector4Length(vector));
        }

        inline bool is_normalized(const VaQuaternionf& quaternion) noexcept {
            DirectX::XMVECTOR vector = va::math::detail::LoadVaQuaternion(quaternion);
            float lengthSq = DirectX::XMVectorGetX(DirectX::XMVector4LengthSq(vector));
            return fabs(1 - lengthSq) <= 1e-4f;
        }

        inline VaQuaternionf from_axis_rotation(const VaVector3f& axis, float angleInRadians) noexcept {
            VaQuaternionf q;
            DirectX::XMVECTOR qv = DirectX::XMQuaternionRotationAxis(va::math::detail::LoadVaVector3(axis), angleInRadians);
            va::math::detail::StoreVaQuaternion(&q, qv);
            return q;
        }

        inline VaQuaternionf from_eular_angles(const VaVector3f& anglesInRadians) noexcept {
            VaQuaternionf q;
            DirectX::XMVECTOR qv = DirectX::XMQuaternionRotationRollPitchYaw(anglesInRadians.x, anglesInRadians.y, anglesInRadians.z);
            va::math::detail::StoreVaQuaternion(&q, qv);
            return q;
        }

        inline VaQuaternionf slerp(const VaQuaternionf& a, const VaQuaternionf& b, float alpha) noexcept {
            DirectX::XMVECTOR qa = va::math::detail::LoadVaQuaternion(a);
            DirectX::XMVECTOR qb = va::math::detail::LoadVaQuaternion(b);
            DirectX::XMVECTOR qr = DirectX::XMQuaternionSlerp(qa, qb, alpha);
            VaQuaternionf result;
            va::math::detail::StoreVaQuaternion(&result, qr);
            return result;
        }

    } // namespace quaternion

    namespace vector {

#define VECTOR3F_OPERATOR(op)                                                    \
    constexpr VaVector3f operator op(const VaVector3f& a, const VaVector3f& b) { \
        return VaVector3f{a.x op b.x, a.y op b.y, a.z op b.z};                   \
    }
        VECTOR3F_OPERATOR(+);
        VECTOR3F_OPERATOR(-);
        VECTOR3F_OPERATOR(*);
        VECTOR3F_OPERATOR(/);
#undef VECTOR3F_OPERATOR

#define VECTOR3F_OPERATOR(op)                                        \
    constexpr VaVector3f operator op(const VaVector3f& a, float s) { \
        return VaVector3f{a.x op s, a.y op s, a.z op s};             \
    }
        VECTOR3F_OPERATOR(+);
        VECTOR3F_OPERATOR(-);
        VECTOR3F_OPERATOR(*);
        VECTOR3F_OPERATOR(/);
#undef VECTOR3F_OPERATOR

#define VECTOR3F_OPERATOR(op)                                        \
    constexpr VaVector3f operator op(float s, const VaVector3f& a) { \
        return VaVector3f{s op a.x, s op a.y, s op a.z};             \
    }
        VECTOR3F_OPERATOR(+);
        VECTOR3F_OPERATOR(-);
        VECTOR3F_OPERATOR(*);
        VECTOR3F_OPERATOR(/);
#undef VECTOR3F_OPERATOR

        constexpr float dot(const VaVector3f& a, const VaVector3f& b) noexcept {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        inline VaVector3f cross(const VaVector3f& a, const VaVector3f& b) noexcept {
            return VaVector3f{a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x};
        }

        inline float length(const VaVector3f& v) noexcept {
            return std::sqrt(dot(v, v));
        }

        inline VaVector3f normalize(const VaVector3f& a) noexcept {
            return a / std::sqrt(dot(a, a));
        }
    } // namespace vector

    namespace pose {
        inline VaPosef Combine(const VaPosef& a, const VaPosef& b) {
            // Q: Quaternion, P: Position, R:Rotation, T:Translation
            //   (Qa Pa) * (Qb Pb)
            //   = Ra * Ta * Rb * Tb
            //   = Ra * (Ta * Rb) * Tb
            //   = Ra * RotationOf(Ta * Rb) * TranslationOf(Ta * Rb) * Tb
            // => Rc = Ra * RotationOf(Ta * Rb)
            //    Qc = Qa * Qb;
            // => Tc = TranslationOf(Ta * Rb) * Tb
            //    Pc = XMVector3Rotate(Pa, Qb) + Pb;

            const DirectX::XMVECTOR pa = math::detail::LoadVaVector3(a.position);
            const DirectX::XMVECTOR qa = math::detail::LoadVaQuaternion(a.orientation);
            const DirectX::XMVECTOR pb = math::detail::LoadVaVector3(b.position);
            const DirectX::XMVECTOR qb = math::detail::LoadVaQuaternion(b.orientation);

            VaPosef c;
            math::detail::StoreVaQuaternion(&c.orientation, DirectX::XMQuaternionMultiply(qa, qb));
            math::detail::StoreVaVector3(&c.position, DirectX::XMVectorAdd(DirectX::XMVector3Rotate(pa, qb), pb));
            return c;
        }

        inline VaPosef Invert(const VaPosef& pose) {
            const DirectX::XMVECTOR orientation = math::detail::LoadVaQuaternion(pose.orientation);
            const DirectX::XMVECTOR invertOrientation = DirectX::XMQuaternionConjugate(orientation);

            const DirectX::XMVECTOR position = math::detail::LoadVaVector3(pose.position);
            const DirectX::XMVECTOR invertPosition = DirectX::XMVector3Rotate(DirectX::XMVectorNegate(position), invertOrientation);

            VaPosef result;
            math::detail::StoreVaQuaternion(&result.orientation, invertOrientation);
            math::detail::StoreVaVector3(&result.position, invertPosition);
            return result;
        }
    } // namespace pose
} // namespace va

#pragma endregion
