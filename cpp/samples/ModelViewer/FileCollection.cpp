#include <memory>
#include <filesystem>

#define VA_ENABLE_DEBUG_PRINTF 1
#include <vaError.h>
#include <vaWindows.h>
#include <vaString.h>
#include <vaUuid.h>

#include "FileCollection.h"

namespace {

    std::vector<ModelData> EnumerateModels(const std::vector<fs::path>& files, const std::vector<fs::path>& folders, bool recursive) {
        std::vector<ModelData> models;

        for (const fs::path& filePath : files) {
            auto utf8Ext = va::wide_to_utf8(filePath.extension().wstring());
            if (fs::is_regular_file(filePath) && (_stricmp(utf8Ext.c_str(), ".glb") == 0)) {
                auto uri = va::windows::FilePathToUri(va::wide_to_utf8(filePath.wstring()));
                auto name = va::wide_to_utf8(filePath.filename().stem().wstring());
                auto nameExt = va::wide_to_utf8(filePath.filename().wstring());
                models.emplace_back(ModelData{uri, name, nameExt});
            } else {
                TRACE("Ignoring invalid file: %s", va::wide_to_utf8(filePath.wstring()).c_str());
            }
        }

        for (const fs::path& folder : folders) {
            if (!fs::is_directory(folder)) {
                TRACE("Skipping non-directory folder entry: %s", va::wide_to_utf8(folder.wstring()).c_str());
                continue;
            }

            auto addIfModel = [&](const fs::path& path) {
                if (path.extension() == ".glb") {
                    auto uri = va::windows::GetLocalAssetUri(va::wide_to_utf8(path.wstring()));
                    auto name = va::wide_to_utf8(path.filename().stem().wstring());
                    auto nameExt = va::wide_to_utf8(path.filename().wstring());
                    models.emplace_back(ModelData{uri, name, nameExt});
                }
            };

            if (recursive) {
                for (fs::recursive_directory_iterator it(folder), end; it != end; ++it) {
                    if (it->is_regular_file()) {
                        addIfModel(it->path());
                    }
                }
            } else {
                for (fs::directory_iterator it(folder), end; it != end; ++it) {
                    if (it->is_regular_file()) {
                        addIfModel(it->path());
                    }
                }
            }
        }

        if (!models.empty()) {
            TRACE("Found %d glb files", models.size());
            for (size_t i = 0; i < models.size(); i++) {
                TRACE("Model[%d]: %s", i, models[i].fileUri.c_str());
            }
        }

        return models;
    }

    // vertical bar (|) is not allowed in URI specification (RFC 3986).
    const std::wstring delimiter = L"|";

    std::wstring serialize(const std::vector<ModelData>& models) {
        std::wstring result;
        for (const auto& model : models) {
            result += va::utf8_to_wide(model.fileUri) + delimiter;
        }
        return result;
    }

    std::vector<std::wstring> deserialize(const std::wstring& serialized) {
        std::vector<std::wstring> result;
        size_t start = 0;
        size_t end = serialized.find(delimiter);

        while (end != std::string::npos) {
            result.push_back(serialized.substr(start, end - start));
            start = end + delimiter.length();
            end = serialized.find(delimiter, start);
        }

        // Donnot forget the remaining part
        result.push_back(serialized.substr(start, end));
        return result;
    }

    std::vector<fs::path> DefaultFolders() {
        std::vector<fs::path> folders;
        folders.push_back(va::windows::GetLocalAssetPath("models"));

        fs::path user3dObjects = va::windows::GetUserProfileDirectory("3D Objects");
        if (!user3dObjects.empty()) {
            folders.push_back(user3dObjects);
        }

        return folders;
    }

    class FileCollection : public IFileCollection {
    public:
        explicit FileCollection(std::vector<ModelData> models)
            : m_models(std::move(models)) {}

        bool TryGetCurrent(ModelData& modelData) const override {
            if (m_currentIndex < m_models.size()) {
                modelData = m_models[m_currentIndex];
                return true;
            }
            return false;
        }

        size_t Count() const override {
            return m_models.size();
        }

        void Next() override {
            if (!m_models.empty()) {
                m_currentIndex = (m_currentIndex + 1) % m_models.size();
            }
        }

        void Previous() override {
            if (!m_models.empty()) {
                m_currentIndex = (m_currentIndex + m_models.size() - 1) % m_models.size();
            }
        }

        void StoreState(const std::wstring& uuid) const override {
            va::windows::WriteSetting(uuid.c_str(), L"currentIndex", std::to_wstring(m_currentIndex).c_str());
            va::windows::WriteSetting(uuid.c_str(), L"models", serialize(m_models).c_str());
        }

        bool RestoreState(const std::wstring& uuid) override {
            bool restoreSuccess = false;
            if (va::windows::TryReadSettingAsUInt64(uuid.c_str(), L"currentIndex", m_currentIndex)) {
                std::wstring serialized;
                if (va::windows::TryReadSetting(uuid.c_str(), L"models", serialized)) {
                    std::vector<std::wstring> uris = deserialize(serialized);
                    for (const auto& uri : uris) {
                        auto uriUtf8 = va::wide_to_utf8(uri);
                        auto filename = fs::path(uriUtf8).filename();
                        auto filenameStem = va::wide_to_utf8(filename.stem().wstring());
                        auto fileNameUtf8 = va::wide_to_utf8(filename.wstring());
                        m_models.emplace_back(ModelData{uriUtf8, filenameStem, fileNameUtf8});
                    }
                    restoreSuccess = true;
                }
            }

            if (!restoreSuccess) {
                TRACE("Failed to restore state, using default values.");
                m_currentIndex = 0;
                m_models = EnumerateModels({}, DefaultFolders(), false);
            }

            return restoreSuccess;
        }

    private:
        size_t m_currentIndex = 0;
        std::vector<ModelData> m_models;
    };

} // namespace

std::unique_ptr<IFileCollection> CreateFileCollection(const std::vector<std::wstring>& argv) {
    std::vector<fs::path> folders;
    std::vector<fs::path> files;
    bool recursive = false;
    for (size_t i = 0; i < argv.size(); i++) {
        TRACE("argv[%d]: %s", i, argv[i].c_str());

        if (i == 0) {
            continue; // path to this exe is ignored.
        }

        if (argv[i] == L"-r" || argv[i] == L"--recursive") {
            recursive = true;
            TRACE("Recursive traversal enabled.");
            continue;
        }

        fs::path path = argv[i];
        auto utf8Path = va::wide_to_utf8(path.wstring());
        auto utf8Ext = va::wide_to_utf8(path.extension().wstring());
        if (fs::is_directory(path)) {
            folders.push_back(path);
        } else if (fs::is_regular_file(path) && (_stricmp(utf8Ext.c_str(), ".glb") == 0)) {
            files.push_back(path);
        } else {
            TRACE("Ignoring invalid path: %s, extension string %s", utf8Path.c_str(), utf8Ext.c_str());
        }
    }

    if (folders.empty() && files.empty()) {
        TRACE("No custom folders provided, using a default folder instead.");
        folders = DefaultFolders();
    }

    return std::make_unique<FileCollection>(EnumerateModels(files, folders, recursive));
}

std::unique_ptr<IFileCollection> LoadFileCollection(const VaUuid& volumeRestoreId) {
    auto uuid = va::to_wstring(volumeRestoreId);
    auto fileCollection = std::make_unique<FileCollection>(std::vector<ModelData>{});
    if (!fileCollection->RestoreState(uuid)) {
        THROW("Failed to restore file collection state.");
    }
    return fileCollection;
}
