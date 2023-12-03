using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace WorkDir.Storage.StorageServices;

public class AzureStorageService : IAzureStorageService
{
    private readonly string? connectionString;
    private readonly string containerName = string.Empty;
    private readonly BlobContainerClient containerClient;

    public AzureStorageService(IConfiguration configuration)
    {
        containerName = configuration.GetSection("BlobStorage").GetValue<string>("ContainerName") ?? throw new ArgumentNullException(nameof(configuration));

        // create a container client object
        connectionString = configuration.GetSection("BlobStorage").GetValue<string>("AzureStorageConnectionString");

        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public void CreateStorage()
    {
        //new storage client
        var blobServiceClient = new BlobServiceClient(connectionString);
        //Create a container
        blobServiceClient.CreateBlobContainer(containerName);
    }

    public void SaveFile(Guid fileName, IFormFile file)
    {
        // Get a reference to a blob
        BlobClient blobClient = containerClient.GetBlobClient(fileName.ToString());

        //Save file Stream to blob
        using var stream = file.OpenReadStream();
        blobClient.Upload(stream, true);
    }

    public byte[] GetFileData(Guid imageGuid)
    {
        var imageBlob = containerClient.GetBlobs(prefix: imageGuid.ToString()).FirstOrDefault();

        var blobClient = containerClient.GetBlobClient(imageBlob.Name);

        using var stream = new MemoryStream();
        blobClient.DownloadTo(stream);
        var filContent = stream.ToArray();

        return filContent;
    }

    public void DeleteImage(Guid imageGuid)
    {
        var imageBlob = containerClient.GetBlobs(prefix: imageGuid.ToString()).First();

        var blobClient = containerClient.GetBlobClient(imageBlob.Name);

        blobClient.DeleteIfExists();
    }
}