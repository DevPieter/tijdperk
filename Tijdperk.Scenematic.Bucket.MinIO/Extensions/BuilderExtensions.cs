using Microsoft.Extensions.DependencyInjection;
using Minio;
using Tijdperk.Scenematic.Bucket.Buckets;
using Tijdperk.Scenematic.Bucket.MinIO.Buckets;
using Tijdperk.Scenematic.Bucket.MinIO.Options;

namespace Tijdperk.Scenematic.Bucket.MinIO.Extensions;

public static class BuilderExtensions
{
    public static void UseScenematicMinIO(this IServiceCollection services, MinioBucketOptions minioOptions)
    {
        services.AddMinio(client => client
            .WithEndpoint(minioOptions.Endpoint)
            .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
            .Build()
        );

        services.AddScoped<ICoverBucket, CoverBucket>();
    }
}