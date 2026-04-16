#ifndef VOLUMETRIC_LOADER_INTERFACE_H_
#define VOLUMETRIC_LOADER_INTERFACE_H_ 1

#ifdef __cplusplus
extern "C" {
#endif

#include "volumetric.h"

#define VA_LOADER_VERSION_1_0 1

#define VA_CURRENT_LOADER_RUNTIME_VERSION 1

#define VA_LOADER_INFO_STRUCT_VERSION 1

#define VA_RUNTIME_INFO_STRUCT_VERSION 1

typedef enum VaLoaderInterfaceStructs {
    VA_LOADER_INTERFACE_STRUCT_UNINITIALIZED = 0,
    VA_LOADER_INTERFACE_STRUCT_LOADER_INFO = 1,
    VA_LOADER_INTERFACE_STRUCT_RUNTIME_REQUEST = 3,
    VA_LOADER_INTERFACE_STRUCTS_MAX_ENUM = 0x7FFFFFFF
} VaLoaderInterfaceStructs;
typedef VaResult(VA_API_PTR* PFN_vaGetFunctionPointer)(VaSession session, const char* name, PFN_vaVoidFunction* function);

typedef struct VaNegotiateLoaderInfo {
    VaLoaderInterfaceStructs structType;
    uint32_t structVersion;
    size_t structSize;
    uint32_t minInterfaceVersion;
    uint32_t maxInterfaceVersion;
    VaVersion minApiVersion;
    VaVersion maxApiVersion;
} VaNegotiateLoaderInfo;

typedef struct VaNegotiateRuntimeRequest {
    VaLoaderInterfaceStructs structType;
    uint32_t structVersion;
    size_t structSize;
    uint32_t runtimeInterfaceVersion;
    VaVersion runtimeApiVersion;
    PFN_vaGetFunctionPointer getFunctionPointer;
} VaNegotiateRuntimeRequest;

typedef VaResult(VA_API_PTR* PFN_vaNegotiateLoaderRuntimeInterface)(const VaNegotiateLoaderInfo* loaderInfo, VaNegotiateRuntimeRequest* runtimeRequest);

#ifdef VA_PROTOTYPES
VaResult VA_API_CALL vaNegotiateLoaderRuntimeInterface(const VaNegotiateLoaderInfo* loaderInfo, VaNegotiateRuntimeRequest* runtimeRequest);
#endif /* !VA_PROTOTYPES */

#ifdef __cplusplus
}
#endif

#endif
