#pragma once

#if !defined(_WIN32)
#error "Unsupported platform"
#endif // _WIN32

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <Windows.h>
#include <shellapi.h> // Required for CommandLineToArgvW
#include <shlobj.h>   // For SHGetFolderPath
#include <sstream>
#include <string>
#include <regex>
#include <thread>
#include <atomic>
#include <future>

#if _HAS_CXX17
#include <filesystem>
namespace fs = std::filesystem;
#else
#define _SILENCE_EXPERIMENTAL_FILESYSTEM_DEPRECATION_WARNING
#include <experimental/filesystem>
#undef _SILENCE_EXPERIMENTAL_FILESYSTEM_DEPRECATION_WARNING
namespace fs = std::experimental::filesystem::v1;
#endif

#include <UserEnv.h>
#include <winhttp.h>

#include <appmodel.h>

#include <volumetric/volumetric_loader_default.h>
#include <vaError.h>
#include <vaString.h>

namespace va {
    namespace detail {
        inline PFN_vaGetFunctionPointer LoadRuntime(VaVersion appApiVersion) {
            const VaVersion minApiVersion = VA_MAKE_VERSION(VA_VERSION_MAJOR(appApiVersion), VA_VERSION_MINOR(0), VA_VERSION_PATCH(0));
            const VaVersion maxApiVersion = VA_MAKE_VERSION(VA_VERSION_MAJOR(appApiVersion), VA_VERSION_MINOR(UINT64_MAX), VA_VERSION_PATCH(UINT64_MAX));

            PFN_vaGetFunctionPointer getFunctionPointer{};
            CHECK_VA(vaNegotiateRuntime(&getFunctionPointer, minApiVersion, maxApiVersion));
            CHECK(getFunctionPointer != nullptr);
            return getFunctionPointer;
        }

        inline std::wstring GetInstalledPackageVersion(const wchar_t* packageFamilyName) {
            UINT32 count = 0;
            UINT32 bufferLength = 0;
            LONG result = FindPackagesByPackageFamily(packageFamilyName, PACKAGE_FILTER_HEAD | PACKAGE_FILTER_DIRECT, &count, nullptr, &bufferLength, nullptr, nullptr);
            if (result != ERROR_INSUFFICIENT_BUFFER || count == 0 || bufferLength == 0) {
                return L"";
            }

            auto buffer = std::make_unique<wchar_t[]>(bufferLength);
            std::vector<PWSTR> fullNames(count);

            result = FindPackagesByPackageFamily(packageFamilyName, PACKAGE_FILTER_HEAD | PACKAGE_FILTER_DIRECT, &count, fullNames.data(), &bufferLength, buffer.get(), nullptr);
            if (result != ERROR_SUCCESS || count == 0) {
                return L"";
            }

            for (UINT32 i = 0; i < count; i++) {
                PACKAGE_INFO_REFERENCE packageInfoRef = nullptr;
                result = OpenPackageInfoByFullName(fullNames[i], 0, &packageInfoRef);
                if (result != ERROR_SUCCESS) {
                    continue;
                }

                UINT32 infoLen = 0;
                UINT32 infoCount = 0;
                result = GetPackageInfo(packageInfoRef, PACKAGE_FILTER_HEAD | PACKAGE_FILTER_DIRECT, &infoLen, nullptr, &infoCount);
                if (result != ERROR_INSUFFICIENT_BUFFER || infoLen == 0) {
                    ClosePackageInfo(packageInfoRef);
                    continue;
                }

                auto infoBuffer = std::make_unique<BYTE[]>(infoLen);
                result = GetPackageInfo(packageInfoRef, PACKAGE_FILTER_HEAD | PACKAGE_FILTER_DIRECT, &infoLen, infoBuffer.get(), &infoCount);
                ClosePackageInfo(packageInfoRef);

                if (result != ERROR_SUCCESS || infoCount == 0) {
                    continue;
                }

                const PACKAGE_INFO* packageInfo = reinterpret_cast<const PACKAGE_INFO*>(infoBuffer.get());
                const PACKAGE_VERSION& version = packageInfo->packageId.version;

                std::wstring versionString =
                    std::to_wstring(version.Major) + L"." + std::to_wstring(version.Minor) + L"." + std::to_wstring(version.Build) + L"." + std::to_wstring(version.Revision);
                return versionString;
            }

            return L"";
        }

        inline std::wstring GetInstalledPackageFullName(const wchar_t* packageFamilyName) {
            UINT32 count = 0;
            UINT32 bufferLength = 0;
            LONG result = FindPackagesByPackageFamily(packageFamilyName, PACKAGE_FILTER_HEAD | PACKAGE_FILTER_DIRECT, &count, nullptr, &bufferLength, nullptr, nullptr);
            if (result != ERROR_INSUFFICIENT_BUFFER || count == 0 || bufferLength == 0) {
                return L"";
            }

            std::unique_ptr<wchar_t[]> buffer = std::make_unique<wchar_t[]>(bufferLength);
            std::vector<PWSTR> fullNames(count);

            result = FindPackagesByPackageFamily(packageFamilyName, PACKAGE_FILTER_HEAD | PACKAGE_FILTER_DIRECT, &count, fullNames.data(), &bufferLength, buffer.get(), nullptr);
            if (result != ERROR_SUCCESS || count == 0) {
                return L"";
            }

            for (UINT32 index = 0; index < count; index++) {
                if (fullNames[index] != nullptr) {
                    return std::wstring(fullNames[index]);
                }
            }

            return L"";
        }

        inline std::wstring QueryStoreApiForVersion(const wchar_t* storeProductId, const wchar_t* packageFamilyName) {
            HINTERNET hSession = ::WinHttpOpen(L"VolumetricSDK/1.0", WINHTTP_ACCESS_TYPE_AUTOMATIC_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);
            if (!hSession) {
                return L"";
            }

            HINTERNET hConnect = ::WinHttpConnect(hSession, L"displaycatalog.mp.microsoft.com", INTERNET_DEFAULT_HTTPS_PORT, 0);
            if (!hConnect) {
                ::WinHttpCloseHandle(hSession);
                return L"";
            }

            std::wstring path = L"/v7.0/products?bigIds=" + std::wstring(storeProductId) + L"&market=US&languages=en-us";
            HINTERNET hRequest = ::WinHttpOpenRequest(hConnect, L"GET", path.c_str(), NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, WINHTTP_FLAG_SECURE);
            if (!hRequest) {
                ::WinHttpCloseHandle(hConnect);
                ::WinHttpCloseHandle(hSession);
                return L"";
            }

            DWORD securityFlags =
                SECURITY_FLAG_IGNORE_UNKNOWN_CA | SECURITY_FLAG_IGNORE_CERT_DATE_INVALID | SECURITY_FLAG_IGNORE_CERT_CN_INVALID | SECURITY_FLAG_IGNORE_CERT_WRONG_USAGE;
            ::WinHttpSetOption(hRequest, WINHTTP_OPTION_SECURITY_FLAGS, &securityFlags, sizeof(securityFlags));

            BOOL result = ::WinHttpSendRequest(hRequest, WINHTTP_NO_ADDITIONAL_HEADERS, 0, WINHTTP_NO_REQUEST_DATA, 0, 0, 0);
            if (!result) {
                ::WinHttpCloseHandle(hRequest);
                ::WinHttpCloseHandle(hConnect);
                ::WinHttpCloseHandle(hSession);
                return L"";
            }

            result = ::WinHttpReceiveResponse(hRequest, NULL);
            if (!result) {
                ::WinHttpCloseHandle(hRequest);
                ::WinHttpCloseHandle(hConnect);
                ::WinHttpCloseHandle(hSession);
                return L"";
            }

            std::string responseData;
            DWORD bytesAvailable = 0;
            DWORD bytesRead = 0;
            char buffer[4096];

            while (::WinHttpQueryDataAvailable(hRequest, &bytesAvailable) && bytesAvailable > 0) {
                DWORD toRead = (bytesAvailable > sizeof(buffer)) ? sizeof(buffer) : bytesAvailable;
                if (::WinHttpReadData(hRequest, buffer, toRead, &bytesRead) && bytesRead > 0) {
                    responseData.append(buffer, bytesRead);
                }
            }

            ::WinHttpCloseHandle(hRequest);
            ::WinHttpCloseHandle(hConnect);
            ::WinHttpCloseHandle(hSession);

            std::string narrowPackageFamilyName = va::wide_to_utf8(packageFamilyName);

            try {
                std::string pattern = "\"PackageFamilyName\":\"" + narrowPackageFamilyName + "\".*?\"Version\":\"(\\d+)\"";
                std::regex versionRegex(pattern);
                std::smatch match;

                if (!std::regex_search(responseData, match, versionRegex) || match.size() < 2) {
                    return L"";
                }

                std::string versionStr = match[1].str();
                uint64_t packedVersion = std::stoull(versionStr);

                uint16_t major = static_cast<uint16_t>((packedVersion >> 48) & 0xFFFF);
                uint16_t minor = static_cast<uint16_t>((packedVersion >> 32) & 0xFFFF);
                uint16_t build = static_cast<uint16_t>((packedVersion >> 16) & 0xFFFF);
                uint16_t revision = static_cast<uint16_t>(packedVersion & 0xFFFF);

                return std::to_wstring(major) + L"." + std::to_wstring(minor) + L"." + std::to_wstring(build) + L"." + std::to_wstring(revision);
            } catch (...) {
                return L"";
            }
        }

    } // namespace detail

    namespace windows {
        inline std::string FilePathToUri(std::string filePath) {
            std::replace(filePath.begin(), filePath.end(), '\\', '/');

            // Spaces to be replaced with %20
            const std::string escapedSpace{"%20"};
            size_t pos = filePath.find(" ");
            while (pos != std::string::npos) {
                filePath.replace(pos, 1, escapedSpace);
                pos = filePath.find(" ");
            }

            return "file:///" + filePath;
        }

        inline fs::path GetLocalAssetPath(const std::string& relativePath) {
            wchar_t wbuffer[MAX_PATH];
            ::GetModuleFileNameW(NULL, wbuffer, MAX_PATH);
            fs::path exePath(wbuffer);
            fs::path folder = exePath.parent_path();
            std::error_code error;
            fs::path assetFile = fs::canonical(folder / relativePath, error);
            if (!error) {
                return assetFile;
            }

            return {};
        }

        inline std::string GetLocalAssetUri(const std::string& relativePath) {
            auto pathWide = GetLocalAssetPath(relativePath).wstring();
            auto pathUtf8 = va::wide_to_utf8(pathWide);
            return FilePathToUri(pathUtf8);
        }

        inline fs::path GetUserProfileDirectory() {
            wchar_t wbuffer[MAX_PATH];
            DWORD bufSize = MAX_PATH;
            if (::GetUserProfileDirectoryW(::GetCurrentProcessToken(), wbuffer, &bufSize)) {
                std::error_code error;
                auto wresult = fs::canonical(wbuffer, error).wstring();
                if (!error) {
                    return wresult;
                }
            }

            return {};
        }

        inline fs::path GetUserProfileDirectory(const std::string& subdirectory) {
            fs::path userProfile{GetUserProfileDirectory()};
            auto user3dModels = userProfile / subdirectory;

            if (fs::exists(user3dModels)) {
                std::error_code error;
                auto wresult = fs::canonical(user3dModels, error).wstring();
                if (!error) {
                    return wresult;
                }
            }

            return {};
        }

        inline std::vector<std::wstring> GetCommandLineArgs() {
            int argc;
            LPWSTR* argv = CommandLineToArgvW(GetCommandLineW(), &argc);
            if (argv == nullptr) {
                return {};
            }

            std::vector<std::wstring> args;
            for (int i = 0; i < argc; i++) {
                args.emplace_back(argv[i]);
            }

            LocalFree(argv);
            return args;
        }

        inline std::wstring GetExecutableFileNameWithoutExtension() {
            wchar_t filename[MAX_PATH];
            if (GetModuleFileNameW(NULL, filename, MAX_PATH)) {
                fs::path path(filename);
                return path.filename().stem().wstring();
            }
            return std::wstring();
        }

        inline std::wstring GetAppDataPath() {
            wchar_t path[MAX_PATH];
            if (SUCCEEDED(SHGetFolderPathW(NULL, CSIDL_APPDATA, NULL, 0, path))) {
                return std::wstring(path);
            } else {
                return std::wstring();
            }
        }

        inline std::wstring GetIniFilePath() {
            static std::wstring settingsFile = fs::path(GetAppDataPath()) / GetExecutableFileNameWithoutExtension() / L"settings.ini";
            return settingsFile;
        }

        inline void WriteSetting(const wchar_t* section, const wchar_t* key, const wchar_t* value) {
            static fs::path settingsParentDirectory = fs::path(GetAppDataPath()) / GetExecutableFileNameWithoutExtension();
            const std::wstring settingsFile = GetIniFilePath();

            fs::create_directories(settingsParentDirectory);
            WritePrivateProfileStringW(section, key, value, settingsFile.c_str());
        }

        inline bool TryReadSetting(const wchar_t* section, const wchar_t* key, std::wstring& value) {
            static std::wstring settingsFile = fs::path(GetAppDataPath()) / GetExecutableFileNameWithoutExtension() / L"settings.ini";

            wchar_t buffer[256];
            DWORD result = GetPrivateProfileStringW(section,                            // Section name
                                                    key,                                // Key name
                                                    value.c_str(),                      // Default value
                                                    buffer,                             // Buffer to hold the read value
                                                    sizeof(buffer) / sizeof(buffer[0]), // Size of the buffer
                                                    settingsFile.c_str()                // Path to the INI file
            );

            if (result == 0) {
                return false; // Failed to read the setting, value is not changed.
            } else {
                value = buffer;
                return true; // Successfully read the setting, value is updated.
            }
        }

        inline bool TryReadSettingAsInt(const wchar_t* section, const wchar_t* key, int& value) {
            try {
                std::wstring wstring = std::to_wstring(value);
                if (TryReadSetting(section, key, wstring)) {
                    value = std::stoi(wstring.c_str());
                    return true;
                }
            } catch (...) {}
            return false;
        }

        inline bool TryReadSettingAsUInt64(const wchar_t* section, const wchar_t* key, uint64_t& value) {
            try {
                std::wstring wstring = std::to_wstring(value);
                if (TryReadSetting(section, key, wstring)) {
                    value = std::stoull(wstring.c_str());
                    return true;
                }
            } catch (...) {}
            return false;
        }

        inline std::vector<std::wstring> ReadSettingsSectionNames() {
            const std::wstring settingsFile = GetIniFilePath();

            std::vector<std::wstring> result;

            // First, determine how much space we need to allocate. GetPrivateProfileSectionNamesW is odd in that it doesn't tell you
            // directly how much space you need. Instead, from the docs:
            //
            // The return value specifies the number of characters copied to the specified buffer, not including the terminating null
            // character. If the buffer is not large enough to contain all the section names associated with the specified initialization
            // file, the return value is equal to the size specified by nSize minus two.

            DWORD size = 2000;
            auto buffer = std::make_unique<wchar_t[]>(size);

            auto sizeRequired = ::GetPrivateProfileSectionNamesW(buffer.get(), size, settingsFile.c_str());

            while (sizeRequired + 2 >= size) {
                size += 2000;
                buffer = std::make_unique<wchar_t[]>(size);
                sizeRequired = ::GetPrivateProfileSectionNamesW(buffer.get(), size, settingsFile.c_str());
            }

            // Now buffer contains the section names. It will be <chars><null><chars><null>...<chars><null><null>
            wchar_t* pstr = buffer.get();
            size_t strlength = wcslen(pstr);

            while (strlength > 0) {
                result.push_back(std::wstring{pstr});
                pstr = &pstr[strlength + 1];
                strlength = wcslen(pstr);
            }

            return result;
        }

        inline void RemoveSection(const wchar_t* section) {
            WritePrivateProfileStringW(section, nullptr, nullptr, GetIniFilePath().c_str());
        }

    } // namespace windows
} // namespace va
