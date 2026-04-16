// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

#include <memory>
#include <stdexcept>
#include <stdarg.h>

#include <volumetric/volumetric.h>
#include "VaEnums.h"

#if defined(_WIN32)
#ifndef NOMINMAX
#define NOMINMAX
#endif
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <Windows.h>
#endif

#define THROW(msg) va::detail::_Throw(msg, nullptr, FILE_AND_LINE)
#define CHECK(exp) va::detail::_Check((exp), "Check failed", #exp, FILE_AND_LINE)
#define CHECK_MSG(exp, msg) va::detail::_Check((exp), msg, #exp, FILE_AND_LINE)
#define THROW_IF_NULL(exp)                                                \
    {                                                                     \
        if ((exp) == nullptr)                                             \
            va::detail::_Throw("Failed null check", #exp, FILE_AND_LINE); \
    } // Fix warning C6011, Dereferencing NULL pointer

#ifndef FAIL_FAST
#define FAIL_FAST(msg)                              \
    {                                               \
        va::detail::_Print(msg, FILE_AND_LINE);     \
        __fastfail(7 /*FAST_FAIL_FATAL_APP_EXIT*/); \
    }
#endif

#define CHECK_VA(cmd) va::detail::_CheckVaResult(cmd, #cmd, FILE_AND_LINE)
#define CHECK_VARESULT(res, cmdStr) va::detail::_CheckVaResult(res, cmdStr, FILE_AND_LINE)
#define THROW_VARESULT(res, cmdStr) va::detail::_ThrowVaResult(res, cmdStr, FILE_AND_LINE)

#ifdef _WIN32
#define CHECK_HR(cmd) va::detail::_CheckHResult(cmd, #cmd, FILE_AND_LINE)
#define CHECK_HRESULT(res, cmdStr) va::detail::_CheckHResult(res, cmdStr, FILE_AND_LINE)
#define THROW_HRESULT(res, cmdStr) va::detail::_ThrowHResult(res, cmdStr, FILE_AND_LINE)
#endif

#define TRACE(...) va::detail::_Print((va::detail::_Fmt(__VA_ARGS__) + "\n").c_str())

#ifndef CATCH_TRACE_IGNORE
#define CATCH_TRACE_IGNORE(name)                    \
    catch (const ::std::exception& ex) {            \
        TRACE(name "_StdException, %s", ex.what()); \
    }                                               \
    catch (...) {                                   \
        TRACE(name "_UnknownException");            \
    }
#endif // !CATCH_TRACE_IGNORE

namespace va {
    class va_error_exception : public std::logic_error {
    public:
        const VaResult Result;

        va_error_exception(VaResult result, const std::string& message)
            : Result(result)
            , std::logic_error(message) {}
    };

    namespace detail {
#define CHK_STRINGIFY(x) #x
#define TOSTRING(x) CHK_STRINGIFY(x)
#define FILE_AND_LINE __FILE__ ":" TOSTRING(__LINE__)

        inline std::string _Fmt(const char* fmt, ...) {
            va_list vl;
            va_start(vl, fmt);
            int size = std::vsnprintf(nullptr, 0, fmt, vl);
            va_end(vl);

            if (size != -1) {
                std::unique_ptr<char[]> buffer(new char[size + 1]);

                va_start(vl, fmt);
                size = std::vsnprintf(buffer.get(), size + 1, fmt, vl);
                va_end(vl);
                if (size != -1) {
                    return std::string(buffer.get(), size);
                }
            }

            throw std::runtime_error("Unexpected vsnprintf failure");
        }

        inline void _OutputDebugString(const char* msg) {
#if defined(DEBUG) && (!defined(VA_DISABLE_DEBUG_OUTPUT) || defined(VA_ENABLE_DEBUG_PRINTF))
#if !defined(VA_DISABLE_DEBUG_OUTPUT)
#ifdef _WIN32
            ::OutputDebugStringA(msg);
#else
#error Unsupported platform.
#endif
#endif
#ifdef VA_ENABLE_DEBUG_PRINTF
            printf("%s", msg);
#endif
#else
            (void)msg;
#endif
        }

        inline void _Print(std::string message, const char* originator = nullptr, const char* sourceLocation = nullptr) {
            if (originator != nullptr) {
                message += _Fmt("\n    Origin: %s\n", originator);
            }
            if (sourceLocation != nullptr) {
                message += _Fmt("\n    Source: %s\n", sourceLocation);
            }
            _OutputDebugString(message.c_str());
        }

        [[noreturn]] inline void _Throw(std::string failureMessage, const char* originator = nullptr, const char* sourceLocation = nullptr) {
            _Print(failureMessage, originator, sourceLocation);
            throw std::logic_error(failureMessage);
        }

        template <typename T>
        inline T&& _Check(T&& res, const char* failureMessage, const char* originator = nullptr, const char* sourceLocation = nullptr) {
            if (!res) {
                va::detail::_Throw(failureMessage, originator, sourceLocation);
            }
            return std::forward<T>(res);
        }

        [[noreturn]] inline void _ThrowVaResult(VaResult res, const char* originator = nullptr, const char* sourceLocation = nullptr) {
            const auto failureMessage = _Fmt("VaResult failure [%s]", va::ToString(res));
            _Print(failureMessage, originator, sourceLocation);
            throw va::va_error_exception(res, failureMessage);
        }

        inline VaResult _CheckVaResult(VaResult res, const char* originator = nullptr, const char* sourceLocation = nullptr) {
            if (VA_FAILED(res)) {
                va::detail::_ThrowVaResult(res, originator, sourceLocation);
            }
            return res;
        }

#ifdef _WIN32
        [[noreturn]] inline void _ThrowHResult(HRESULT hr, const char* originator = nullptr, const char* sourceLocation = nullptr) {
            va::detail::_Throw(va::detail::_Fmt("HRESULT failure [%x]", hr), originator, sourceLocation);
        }

        inline HRESULT _CheckHResult(HRESULT hr, const char* originator = nullptr, const char* sourceLocation = nullptr) {
            if (FAILED(hr)) {
                va::detail::_ThrowHResult(hr, originator, sourceLocation);
            }
            return hr;
        }
#endif

    } // namespace detail
} // namespace va
