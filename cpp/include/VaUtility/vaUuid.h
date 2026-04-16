// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

#include <functional>
#include <string.h>

#include <vaString.h>

#ifdef _WIN32
#include <guiddef.h>    // For GUID
#include <combaseapi.h> // For StringFromGUID2
#endif

namespace va {
    namespace details {
        template <typename T>
        class guid_parser {
            typedef _Inout_                                 // Pointer to buffer (cursor itself) is read, and incremented
                _Deref_pre_z_                               // Buffer itself is initially null terminated
                    _When_(**(_Curr_) != 0, _Deref_post_z_) // Buffer remains null terminated unless it matches the null terminator
                const T** BufferCursor;

            static constexpr bool TryParseChar(BufferCursor str, T c) {
                if (**str == c) {
                    ++(*str);
                    return true;
                }
                return false;
            }

            static constexpr void ParseChar(BufferCursor str, T c) {
                if (!TryParseChar(str, c)) {
                    throw std::invalid_argument("unexpected character in guid string");
                }
            }

            static constexpr uint8_t ParseHalfByte(BufferCursor str) {
                // clang-format off
                const T c = *(*str)++;
                return static_cast<uint8_t>(
                    ((c >= '0' && c <= '9')
                         ? (c - '0')
                         : ((c >= 'a' && c <= 'f')
                                ? (c - 'a' + 10)
                                : ((c >= 'A' && c <= 'F') ? (c - 'A' + 10)
                                                          : throw std::invalid_argument("invalid hexadecimal character in guid string")))));
                // clang-format on
            }

            static constexpr uint8_t ParseByte(BufferCursor str) {
                const uint8_t first = ParseHalfByte(str);
                const uint8_t second = ParseHalfByte(str);
                return (first << 4) | second;
            }

            static constexpr uint16_t ParseUInt16(BufferCursor str) {
                const uint8_t upper = ParseByte(str);
                const uint8_t lower = ParseByte(str);
                return (upper << 8) | lower;
            }

            static constexpr uint32_t ParseUInt32(BufferCursor str) {
                const uint16_t high = ParseUInt16(str);
                const uint16_t low = ParseUInt16(str);
                return (high << 16) | low;
            }

        public:
            static constexpr GUID parse(_In_z_ const T* str) {
                const bool hasBraces = TryParseChar(&str, '{');
                const GUID parsed{ParseUInt32(&str),
                                  (ParseChar(&str, '-'), ParseUInt16(&str)),
                                  (ParseChar(&str, '-'), ParseUInt16(&str)),
                                  {
                                      (ParseChar(&str, '-'), ParseByte(&str)),
                                      ParseByte(&str),
                                      (ParseChar(&str, '-'), ParseByte(&str)),
                                      ParseByte(&str),
                                      ParseByte(&str),
                                      ParseByte(&str),
                                      ParseByte(&str),
                                      ParseByte(&str),
                                  }};

                if (hasBraces) {
                    ParseChar(&str, '}');
                }
                ParseChar(&str, '\0');

                return parsed;
            }
        };
    } // namespace details
    // https://en.wikipedia.org/wiki/Universally_unique_identifier#Endianess
    // When saving UUIDs to binary format, they are sequentially encoded in big-endian.
    // For example, 00112233-4455-6677-8899-aabbccddeeff is encoded
    // as the bytes 00 11 22 33 44 55 66 77 88 99 aa bb cc dd ee ff
    //
    // An exception to this are Microsoft's variant 2 UUIDs ("GUID"):
    // historically used in COM/OLE libraries, they use a little-endian format,
    // but appear mixed-endian with the first three components of the UUID as little-endian and last two big-endian,
    // due to the missing byte dashes when formatted as a string.
    // For example, 00112233-4455-6677-8899-aabbccddeeff is encoded
    // as the bytes 33 22 11 00 55 44 77 66 88 99 aa bb cc dd ee ff

    inline constexpr GUID ToGuid(const VaUuid& src) noexcept {
        GUID dest{};
        const auto& u = src.data;

        dest.Data1 = u[0] * 0x01000000u + u[1] * 0x00010000u + u[2] * 0x00000100u + u[3];
        dest.Data2 = uint16_t(u[4] * 0x0100u + u[5]);
        dest.Data3 = uint16_t(u[6] * 0x0100u + u[7]);
        dest.Data4[0] = u[8];
        dest.Data4[1] = u[9];
        dest.Data4[2] = u[10];
        dest.Data4[3] = u[11];
        dest.Data4[4] = u[12];
        dest.Data4[5] = u[13];
        dest.Data4[6] = u[14];
        dest.Data4[7] = u[15];
        return dest;
    }

    inline constexpr VaUuid FromGuid(const GUID& src) noexcept {
        VaUuid dest{};
        auto& u = dest.data;

        u[0] = uint8_t(src.Data1 >> 24);
        u[1] = uint8_t(src.Data1 >> 16);
        u[2] = uint8_t(src.Data1 >> 8);
        u[3] = uint8_t(src.Data1);

        u[4] = uint8_t(src.Data2 >> 8);
        u[5] = uint8_t(src.Data2);

        u[6] = uint8_t(src.Data3 >> 8);
        u[7] = uint8_t(src.Data3);

        u[8] = src.Data4[0];
        u[9] = src.Data4[1];
        u[10] = src.Data4[2];
        u[11] = src.Data4[3];
        u[12] = src.Data4[4];
        u[13] = src.Data4[5];
        u[14] = src.Data4[6];
        u[15] = src.Data4[7];
        return dest;
    }

    // Parses a string of the form {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx} into a guid value (with or without braces).
    // Supports both compile-time and runtime parsing.
    template <typename T>
    inline constexpr GUID parse_guid(_In_z_ const T* str) {
        return details::guid_parser<T>::parse(str);
    }

    template <typename T>
    inline bool try_parse_guid(_In_z_ const T* str, _Out_ GUID* pGuid) noexcept {
        try {
            *pGuid = details::guid_parser<T>::parse(str);
            return true;
        } catch (...) {
            *pGuid = {};
            return false;
        }
    }

    inline std::wstring to_wstring(const VaUuid& uuid) {
        GUID guid = ToGuid(uuid);

        wchar_t guidString[39]; // GUID string length is 38 plus null terminator
        HRESULT hr = StringFromGUID2(guid, guidString, sizeof(guidString) / sizeof(guidString[0]));
        if (SUCCEEDED(hr)) {
            return std::wstring(guidString);
        } else {
            // Handle the error, return an empty string or an error message
            return L"";
        }
    }

    inline VaUuid from_wstring(const std::wstring& str) {
        GUID g{};
        // try_parse_guid will set g to {} upon failure
        (void)try_parse_guid(str.c_str(), &g);
        return FromGuid(g);
    }

    inline std::string to_string(const VaUuid& uuid) {
        return va::wide_to_utf8(to_wstring(uuid));
    }

    inline VaUuid from_string(const std::string& str) {
        return from_wstring(va::utf8_to_wide(str));
    }

    inline int uuid_compare(const VaUuid& a, const VaUuid& b) noexcept {
        return std::memcmp(a.data, b.data, sizeof(VaUuid::data));
    }

    // Enable VaUuid to be used as key in unordered_map/set
    struct VaUuidEq {
        bool operator()(const VaUuid& a, const VaUuid& b) const noexcept {
            return uuid_compare(a, b) == 0;
        }
    };

    // Enable VaUuid to be used as key in unordered_map/set
    // Example: std::unordered_map<VaUuid, ValueType, va::VaUuidHash, va::VaUuidEq> idMap;
    struct VaUuidHash {
        std::size_t operator()(const VaUuid& id) const noexcept {
            std::size_t h = 1469598103934665603ull;
            for (auto b : id.data) {
                h ^= b;
                h *= 1099511628211ull;
            }
            return h;
        }
    };
} // namespace va
