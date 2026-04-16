#pragma once

#include <volumetric/volumetric.h>
#include <volumetric/volumetric_reflection.h>

#include <vaCtor.h>

namespace va {

    class Functions : NonMovable {
    private:
        const VaSession m_session;
        const PFN_vaGetFunctionPointer m_getFunctionPointer;

    public:
#define FUNCTION_TABLE_MEMBER(name) PFN_##name name{nullptr};
#define FUNCTION_TABLE_MEMBER_VOID(name) PFN_vaVoidFunction name{nullptr};

        LIST_FUNCTIONS_VA_CORE(FUNCTION_TABLE_MEMBER);
        LIST_FUNCTIONS_VA_EXTENSIONS(FUNCTION_TABLE_MEMBER, FUNCTION_TABLE_MEMBER_VOID);

#undef FUNCTION_TABLE_MEMBER_VOID
#undef FUNCTION_TABLE_MEMBER

        template <typename PFN>
        PFN GetSessionFunctionPtr(const char* name) const {
            static_assert(std::is_pointer<PFN>::value, "PFN must be a pointer type");
            static_assert(std::is_function<typename std::remove_pointer<PFN>::type>::value, "PFN must be a function pointer type");
            PFN_vaVoidFunction function = nullptr;
            (void)m_getFunctionPointer(m_session, name, &function);
            return reinterpret_cast<PFN>(function);
        }

        Functions(VaSession session, PFN_vaGetFunctionPointer _getFunctionPointer)
            : m_getFunctionPointer(_getFunctionPointer)
            , m_session(session) {
#define FUNCTION_TABLE_GET_PFN(name) name = GetSessionFunctionPtr<PFN_##name>(#name);
#define FUNCTION_TABLE_NO_OP(name)

            LIST_FUNCTIONS_VA_CORE(FUNCTION_TABLE_GET_PFN);
            LIST_FUNCTIONS_VA_EXTENSIONS(FUNCTION_TABLE_GET_PFN, FUNCTION_TABLE_NO_OP);

#undef FUNCTION_TABLE_GET_PFN
#undef FUNCTION_TABLE_NO_OP
        }
    };

} // namespace va
