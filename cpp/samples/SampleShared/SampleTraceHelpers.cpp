#include "SampleTraceHelpers.h"
#include <cstdarg>
#include <cstdio>

#if !defined(_WIN32)
#error "Unsupported platform"
#endif // _WIN32

#include <windows.h>
#include <evntrace.h>
#include <TraceLoggingProvider.h>

// Provider GUID {e86a91a5-41a8-5942-a79e-a4cf1b798f9b}
TRACELOGGING_DEFINE_PROVIDER(g_hProvider, "Microsoft.MixedReality.Volumetric.VaSDKCpp", (0xe86a91a5, 0x41a8, 0x5942, 0xa7, 0x9e, 0xa4, 0xcf, 0x1b, 0x79, 0x8f, 0x9b));

namespace sample {

    void vaTraceInit() {
        TraceLoggingRegister(g_hProvider);
    }

    void vaTraceTerminate() {
        TraceLoggingUnregister(g_hProvider);
    }

    void vaTraceVerbose(const char* format, ...) {
        if (!TraceLoggingProviderEnabled(g_hProvider, 0, 0)) {
            return;
        }

        va_list args;
        va_start(args, format);
        const int length = std::vsnprintf(nullptr, 0, format, args);
        va_end(args);

        if (length > 0) {
            std::string message(length + 1, '\0');
            va_start(args, format);
            std::vsnprintf(&message[0], length + 1, format, args);
            va_end(args);
            message.resize(length); // Remove extra null terminator added by vsnprintf

            TraceLoggingWrite(g_hProvider, "vaCppTrace", TraceLoggingLevel(TRACE_LEVEL_VERBOSE), TraceLoggingUtf8String(message.c_str(), "m"));
        }
    }

    void vaTraceInfo(const char* format, ...) {
        if (!TraceLoggingProviderEnabled(g_hProvider, 0, 0)) {
            return;
        }

        va_list args;
        va_start(args, format);
        const int length = std::vsnprintf(nullptr, 0, format, args);
        va_end(args);

        if (length > 0) {
            std::string message(length + 1, '\0');
            va_start(args, format);
            std::vsnprintf(&message[0], length + 1, format, args);
            va_end(args);
            message.resize(length); // Remove extra null terminator added by vsnprintf

            TraceLoggingWrite(g_hProvider, "vaCppTrace", TraceLoggingLevel(TRACE_LEVEL_INFORMATION), TraceLoggingUtf8String(message.c_str(), "m"));
        }
    }

    void vaTraceWarning(const char* format, ...) {
        if (!TraceLoggingProviderEnabled(g_hProvider, 0, 0)) {
            return;
        }

        va_list args;
        va_start(args, format);
        const int length = std::vsnprintf(nullptr, 0, format, args);
        va_end(args);

        if (length > 0) {
            std::string message(length + 1, '\0');
            va_start(args, format);
            std::vsnprintf(&message[0], length + 1, format, args);
            va_end(args);
            message.resize(length); // Remove extra null terminator added by vsnprintf

            TraceLoggingWrite(g_hProvider, "vaCppTrace", TraceLoggingLevel(TRACE_LEVEL_WARNING), TraceLoggingUtf8String(message.c_str(), "m"));
        }
    }

    void vaTraceError(const char* format, ...) {
        if (!TraceLoggingProviderEnabled(g_hProvider, 0, 0)) {
            return;
        }

        va_list args;
        va_start(args, format);
        const int length = std::vsnprintf(nullptr, 0, format, args);
        va_end(args);

        if (length > 0) {
            std::string message(length + 1, '\0');
            va_start(args, format);
            std::vsnprintf(&message[0], length + 1, format, args);
            va_end(args);
            message.resize(length); // Remove extra null terminator added by vsnprintf

            TraceLoggingWrite(g_hProvider, "vaCppTrace", TraceLoggingLevel(TRACE_LEVEL_ERROR), TraceLoggingUtf8String(message.c_str(), "m"));
        }
    }

    void vaTraceStart(const char* format, ...) {
        if (!TraceLoggingProviderEnabled(g_hProvider, 0, 0)) {
            return;
        }

        va_list args;
        va_start(args, format);
        const int length = std::vsnprintf(nullptr, 0, format, args);
        va_end(args);

        if (length > 0) {
            std::string message(length + 1, '\0');
            va_start(args, format);
            std::vsnprintf(&message[0], length + 1, format, args);
            va_end(args);
            message.resize(length); // Remove extra null terminator added by vsnprintf
            TraceLoggingWrite(
                g_hProvider, "vaCppTrace", TraceLoggingOpcode(EVENT_TRACE_TYPE_START), TraceLoggingLevel(TRACE_LEVEL_INFORMATION), TraceLoggingUtf8String(message.c_str(), "m"));
        }
    }

    void vaTraceStop(const char* format, ...) {
        if (!TraceLoggingProviderEnabled(g_hProvider, 0, 0)) {
            return;
        }

        va_list args;
        va_start(args, format);
        const int length = std::vsnprintf(nullptr, 0, format, args);
        va_end(args);

        if (length > 0) {
            std::string message(length + 1, '\0');
            va_start(args, format);
            std::vsnprintf(&message[0], length + 1, format, args);
            va_end(args);
            message.resize(length); // Remove extra null terminator added by vsnprintf

            TraceLoggingWrite(
                g_hProvider, "vaCppTrace", TraceLoggingOpcode(EVENT_TRACE_TYPE_STOP), TraceLoggingLevel(TRACE_LEVEL_INFORMATION), TraceLoggingUtf8String(message.c_str(), "m"));
        }
    }

    ScopedTrace::ScopedTrace(const char* format, ...) {
        if (!TraceLoggingProviderEnabled(g_hProvider, 0, 0)) {
            return;
        }

        // Format the message using variadic arguments
        va_list args;
        va_start(args, format);

        // Get the required buffer size
        const int length = std::vsnprintf(nullptr, 0, format, args);
        va_end(args);

        if (length > 0) {
            m_traceMessage.resize(length + 1);
            va_start(args, format);
            std::vsnprintf(&m_traceMessage[0], length + 1, format, args);
            va_end(args);

            // Remove extra null terminator added by vsnprintf
            m_traceMessage.resize(length);

            vaTraceStart("%s", m_traceMessage.c_str());
        }
    }

    ScopedTrace::~ScopedTrace() {
        if (!TraceLoggingProviderEnabled(g_hProvider, 0, 0)) {
            return;
        }

        if (!m_traceMessage.empty()) {
            vaTraceStop("%s", m_traceMessage.c_str());
        }
    }

} // namespace sample
