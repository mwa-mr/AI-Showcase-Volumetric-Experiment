#pragma once
#include <string>

namespace sample {
    void vaTraceInit();
    void vaTraceTerminate();

    void vaTraceVerbose(const char* format, ...);
    void vaTraceInfo(const char* format, ...);
    void vaTraceWarning(const char* format, ...);
    void vaTraceError(const char* format, ...);

    void vaTraceStart(const char* format, ...);
    void vaTraceStop(const char* format, ...);

    class ScopedTrace {
    public:
        explicit ScopedTrace(const char* format, ...);
        ~ScopedTrace();

        // Non-copyable and non-movable
        ScopedTrace(const ScopedTrace&) = delete;
        ScopedTrace& operator=(const ScopedTrace&) = delete;
        ScopedTrace(ScopedTrace&&) = delete;
        ScopedTrace& operator=(ScopedTrace&&) = delete;

    private:
        std::string m_traceMessage;
    };
} // namespace sample
