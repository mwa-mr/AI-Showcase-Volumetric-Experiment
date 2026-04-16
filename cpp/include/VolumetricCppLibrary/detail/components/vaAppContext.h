#pragma once

#include <vaFunctions.h>
#include <vaExtensions.h>

namespace va {
    class VolumetricApp;

    namespace detail {
        class Session;

        struct AppContext : Copyable {
            AppContext(VolumetricApp& _app, Session& _session, const Extensions& _extensions, const Functions& _functions)
                : app(_app)
                , session(_session)
                , ext(_extensions)
                , pfn(_functions) {}

            VolumetricApp& app;
            Session& session;
            const Extensions& ext;
            const Functions& pfn;
        };

    } // namespace detail
} // namespace va
