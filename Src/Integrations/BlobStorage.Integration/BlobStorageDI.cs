using Amazon.Runtime;
using Amazon.S3;
using BlobStorage.Configuration;
using BlobStorage.Contracts;
using BlobStorage.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BlobStorage;

/// <summary>
/// Dependency-injection wiring for the S3-compatible (SeaweedFS) blob storage integration.
/// </summary>
public static class BlobStorageDI
{
    /// <summary>
    /// Registers the blob storage options, the underlying <see cref="IAmazonS3"/> client
    /// pointed at the configured SeaweedFS endpoint, and <see cref="IBlobStorageService"/>.
    /// </summary>
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            BlobStorageOptions options = sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = options.ForcePathStyle,
                AuthenticationRegion = options.Region,
            };

            var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);

            return new AmazonS3Client(credentials, config);
        });

        services.AddSingleton<IBlobStorageService, SeaweedBlobStorageService>();

        return services;
    }
}
