#pragma once

#if _HAS_CXX17
#include <string_view>
#else
#include <string>
#endif

#ifdef USE_CODECVT
// the <codecvt> header are deprecated in C++17.
#define _SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING
#include <codecvt>
#endif // USE_CODECVT

#include "vaError.h"

namespace va {
#if _HAS_CXX17
    using string_view_t = std::string_view;
    using wstring_view_t = std::wstring_view;
#define STRING_DATA(str) str.data()
#else
    using string_view_t = const std::string&;
    using wstring_view_t = const std::wstring&;
#define STRING_DATA(str) &str[0]
#endif
    namespace detail {
#ifdef _WIN32
        namespace windows {
            inline std::wstring utf8_to_wide(string_view_t utf8Text) {
                if (utf8Text.empty()) {
                    return {};
                }

                std::wstring wideText;
                const int wideLength = ::MultiByteToWideChar(CP_UTF8, 0, utf8Text.data(), (int)utf8Text.size(), nullptr, 0);
                if (wideLength == 0) {
                    TRACE("utf8_to_wide get size error.");
                    return {};
                }

                // MultiByteToWideChar returns number of chars of the input buffer, regardless of null terminitor
                wideText.resize(wideLength);
                const int length = ::MultiByteToWideChar(CP_UTF8, 0, utf8Text.data(), (int)utf8Text.size(), STRING_DATA(wideText), wideLength);
                if (length != wideLength) {
                    TRACE("utf8_to_wide convert string error.");
                    return {};
                }

                return wideText;
            }

            inline std::string wide_to_utf8(wstring_view_t wideText) {
                if (wideText.empty()) {
                    return {};
                }

                std::string narrowText;
                int narrowLength = ::WideCharToMultiByte(CP_UTF8, 0, wideText.data(), (int)wideText.size(), nullptr, 0, nullptr, nullptr);
                if (narrowLength == 0) {
                    TRACE("wide_to_utf8 get size error.");
                    return {};
                }

                // WideCharToMultiByte returns number of chars of the input buffer, regardless of null terminitor
                narrowText.resize(narrowLength, 0);
                const int length = ::WideCharToMultiByte(CP_UTF8, 0, wideText.data(), (int)wideText.size(), STRING_DATA(narrowText), narrowLength, nullptr, nullptr);
                if (length != narrowLength) {
                    TRACE("wide_to_utf8 convert string error.");
                    return {};
                }

                return narrowText;
            }
        } // namespace windows
#endif

#ifdef USE_CODECVT
        namespace codecvt {
            inline std::wstring utf8_to_wide(string_view_t utf8Text) {
                // Note : std::wstring_convert is deprecated as of cpp17. A replacement for it has not yet been added
                // to the cpp standard. The functions in XrString.h use the 'MultiByte' functions which are only available
                // on Windows.
                std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
                return converter.from_bytes(utf8Text.data(), utf8Text.data() + utf8Text.length());
            }

            inline std::string wide_to_utf8(wstring_view_t wideText) {
                // Note : std::wstring_convert is deprecated as of cpp17. A replacement for it has not yet been added
                // to the cpp standard. The functions in XrString.h use the 'MultiByte' functions which are only available
                // on Windows.
                std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
                return converter.to_bytes(wideText.data(), wideText.data() + wideText.length());
            }
        } // namespace codecvt
#endif
    } // namespace detail

    inline std::wstring utf8_to_wide(string_view_t utf8Text) {
#if defined(_WIN32)
        return detail::windows::utf8_to_wide(utf8Text);
#elif defined(USE_CODECVT)
        return detail::codecvt::utf8_to_wide(utf8Text);
#else
#error Unsupported platform
#endif
    }

    inline std::string wide_to_utf8(wstring_view_t wideText) {
#if defined(_WIN32)
        return detail::windows::wide_to_utf8(wideText);
#elif defined(USE_CODECVT)
        return detail::codecvt::wide_to_utf8(wideText);
#else
#error Unsupported platform
#endif
    }

#ifdef __cpp_lib_char8_t
    inline std::wstring utf8_to_wide(std::u8string_view utf8Text) {
        static_assert(sizeof(char) == sizeof(char8_t));
        return utf8_to_wide(string_view_t(reinterpret_cast<const char*>(utf8Text.data()), utf8Text.size()));
    }
#endif
} // namespace va

#undef STRING_DATA
