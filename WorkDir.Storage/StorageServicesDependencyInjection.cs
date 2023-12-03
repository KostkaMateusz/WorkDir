using Microsoft.Extensions.DependencyInjection;
using WorkDir.Storage.StorageServices;

namespace WorkDir.Storage;

public static class StorageServicesDependencyInjection
{
    public static IServiceCollection AddAzureStorageService(this IServiceCollection services)
    {
        services.AddScoped<IAzureStorageService, AzureStorageService>();

        return services;
    }
}