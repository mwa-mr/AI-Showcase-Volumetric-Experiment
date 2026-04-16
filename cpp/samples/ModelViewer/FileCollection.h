#pragma once

#include <string>
#include <vector>
#include <optional>

struct ModelData {
    std::string fileUri;
    std::string fileName;
    std::string fileNameExt;
};

/// <summary>
/// This helper class collect all glb files from the provided folders.
/// It allows getting the current model and iterating through the collection.
/// </summary>
class IFileCollection {
public:
    virtual ~IFileCollection() = default;

    virtual bool TryGetCurrent(ModelData&) const = 0;

    virtual size_t Count() const = 0;

    virtual void Next() = 0;
    virtual void Previous() = 0;

    virtual void StoreState(const std::wstring& uuid) const = 0;
    virtual bool RestoreState(const std::wstring& uuid) = 0;
};

std::unique_ptr<IFileCollection> CreateFileCollection(const std::vector<std::wstring>& argv);
std::unique_ptr<IFileCollection> LoadFileCollection(const VaUuid& volumeRestoreId);
