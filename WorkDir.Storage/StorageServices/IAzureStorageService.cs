using Microsoft.AspNetCore.Http;

namespace WorkDir.Storage.StorageServices;

public interface IAzureStorageService
{
    void DeleteImage(Guid imageGuid);
    byte[] GetFileData(Guid imageGuid);
    void SaveFile(Guid fileName, IFormFile file);
}