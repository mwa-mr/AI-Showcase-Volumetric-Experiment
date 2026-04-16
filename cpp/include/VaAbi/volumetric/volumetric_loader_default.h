#ifndef VOLUMETRIC_LOADER_DEFAULT_H_
#define VOLUMETRIC_LOADER_DEFAULT_H_ 1

#if !defined(_WIN32)
#error "This header is Windows-only"
#endif

#include "volumetric.h"
#include "volumetric_loader_interface.h"

#ifndef NOMINMAX
#define NOMINMAX
#endif
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <Windows.h>
#include <appmodel.h>

// PackageDependency APIs requires onecoreuap.lib
#pragma comment(lib, "onecoreuap.lib")

#ifdef __cplusplus
extern "C" {
#endif

typedef struct {
    wchar_t packageName[MAX_PATH + 1];
    wchar_t runtimeDllPath[MAX_PATH + 1];
} RuntimePackageInfo;

inline bool TryGetDefaultPackageInfo(RuntimePackageInfo* packageInfo) {
    const wchar_t* DEFAULT_VOLUMETRIC_PACKAGE = L"Microsoft.MixedRealityVolumetric_8wekyb3d8bbwe";
    const wchar_t* DEFAULT_RUNTIME_DLL_PATH = L"Platform\\VolumetricRuntime.dll";
    wcscpy_s(packageInfo->packageName, MAX_PATH + 1, DEFAULT_VOLUMETRIC_PACKAGE);
    wcscpy_s(packageInfo->runtimeDllPath, MAX_PATH + 1, DEFAULT_RUNTIME_DLL_PATH);
    return true;
}

inline bool TryGetPackageInfo(RuntimePackageInfo* packageInfo) {
    const wchar_t* RUNTIME_VOLUMETRIC_REGKEY_PATH = L"SOFTWARE\\Microsoft\\MixedReality\\Volumetric\\";
    const wchar_t* VOLUMETRIC_PACKAGE_REGKEY_VALUE = L"ActiveVolumetricPackage";
    const wchar_t* RUNTIME_PATH_REGKEY_VALUE_NAME = L"ActiveVolumetricRuntime";

    HKEY hKey = NULL;
    LONG result = RegOpenKeyExW(HKEY_CURRENT_USER, RUNTIME_VOLUMETRIC_REGKEY_PATH, 0, KEY_READ, &hKey);
    if (result != ERROR_SUCCESS) {
        return false; // The registry key is not accessible
    }

    DWORD bufferSize = sizeof(packageInfo->packageName);
    result = RegQueryValueExW(hKey, VOLUMETRIC_PACKAGE_REGKEY_VALUE, NULL, NULL, (LPBYTE)packageInfo->packageName, &bufferSize);
    if (result != ERROR_SUCCESS) {
        RegCloseKey(hKey);
        return false; // The package name is not set in the registry
    }

    bufferSize = sizeof(packageInfo->runtimeDllPath);
    result = RegQueryValueExW(hKey, RUNTIME_PATH_REGKEY_VALUE_NAME, NULL, NULL, (LPBYTE)packageInfo->runtimeDllPath, &bufferSize);
    if (result != ERROR_SUCCESS) {
        RegCloseKey(hKey);
        return false; // The runtime dll path is not set in the registry
    }

    // if an absolute path or a ./ path then fail
    if (wcslen(packageInfo->runtimeDllPath) < 2 || packageInfo->runtimeDllPath[0] == L'.' || packageInfo->runtimeDllPath[1] == L'\\' || packageInfo->runtimeDllPath[1] == L'/' ||
        packageInfo->runtimeDllPath[1] == L':') {
        RegCloseKey(hKey);
        return false; // The path must be relative to the package
    }

    RegCloseKey(hKey);
    return true;
}

inline HMODULE LoadRuntimeDll() {
    HMODULE hModule = NULL;
    RuntimePackageInfo packageInfo = {};

    if (TryGetPackageInfo(&packageInfo) || TryGetDefaultPackageInfo(&packageInfo)) {
        if (packageInfo.packageName[0] != L'\0' && packageInfo.runtimeDllPath[0] != L'\0') {
#ifdef _M_X64
            const PackageDependencyProcessorArchitectures arch = PackageDependencyProcessorArchitectures_X64;
#else
            const PackageDependencyProcessorArchitectures arch = PackageDependencyProcessorArchitectures_Arm64;
#endif

            PWSTR packageDependencyId = NULL;
            PACKAGE_VERSION minVersion = {0};
            HRESULT hr = TryCreatePackageDependency(
                NULL, packageInfo.packageName, minVersion, arch, PackageDependencyLifetimeKind_Process, NULL, CreatePackageDependencyOptions_None, &packageDependencyId);

            PACKAGEDEPENDENCY_CONTEXT packageDependencyContext = NULL;
            PWSTR packageFullName = NULL;
            if (SUCCEEDED(hr) && packageDependencyId) {
                hr = AddPackageDependency(packageDependencyId, PACKAGE_DEPENDENCY_RANK_DEFAULT, AddPackageDependencyOptions_None, &packageDependencyContext, &packageFullName);
            }

            if (SUCCEEDED(hr) && packageFullName) {
                hModule = LoadLibraryExW(packageInfo.runtimeDllPath, NULL, 0);
            }

            if (packageFullName != NULL) {
                HeapFree(GetProcessHeap(), 0, packageFullName);
            }
            if (packageDependencyId != NULL) {
                HeapFree(GetProcessHeap(), 0, packageDependencyId);
            }
            if (packageDependencyContext != NULL) {
                RemovePackageDependency(packageDependencyContext);
            }
        }
    }

    return hModule;
}

inline VaResult vaNegotiateRuntime(PFN_vaGetFunctionPointer* vaGetFunctionPointer, VaVersion minApiVersion, VaVersion maxApiVersion) {
    VaResult vaResult = VA_ERROR_RUNTIME_UNAVAILABLE;

    // Load the runtime dll and get function pointer to vaNegotiateLoaderRuntimeInterface
    PFN_vaNegotiateLoaderRuntimeInterface vaNegotiateLoaderRuntimeInterface = NULL;
    HMODULE hModule = LoadRuntimeDll();
    if (hModule != NULL) {
        vaNegotiateLoaderRuntimeInterface = (PFN_vaNegotiateLoaderRuntimeInterface)GetProcAddress(hModule, "vaNegotiateLoaderRuntimeInterface");
    }

    // Negotiate API versions to determine valid runtime entry point
    if (vaNegotiateLoaderRuntimeInterface) {
        VaNegotiateLoaderInfo loaderInfo = {VA_LOADER_INTERFACE_STRUCT_LOADER_INFO};
        loaderInfo.structVersion = VA_LOADER_INFO_STRUCT_VERSION;
        loaderInfo.structSize = sizeof(VaNegotiateLoaderInfo);
        loaderInfo.minInterfaceVersion = 1;
        loaderInfo.maxInterfaceVersion = VA_CURRENT_LOADER_RUNTIME_VERSION;
        loaderInfo.minApiVersion = minApiVersion;
        loaderInfo.maxApiVersion = maxApiVersion;

        VaNegotiateRuntimeRequest runtimeRequest = {VA_LOADER_INTERFACE_STRUCT_RUNTIME_REQUEST};
        runtimeRequest.structVersion = VA_RUNTIME_INFO_STRUCT_VERSION;
        runtimeRequest.structSize = sizeof(VaNegotiateRuntimeRequest);

        vaResult = vaNegotiateLoaderRuntimeInterface(&loaderInfo, &runtimeRequest);
        if (VA_SUCCEEDED(vaResult)) {
            *vaGetFunctionPointer = runtimeRequest.getFunctionPointer;
        }
    }

    return vaResult;
}
#ifdef __cplusplus
}
#endif

#endif
