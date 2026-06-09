using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tijdperk.Scenematic.Bucket.Options;

namespace Tijdperk.Scenematic.Bucket.Extensions;

public static class BuilderExtensions
{
    public static void RegisterBucketOptions(this IHostApplicationBuilder builder, string bucketOptionsSectionName)
    {
        builder.Services.Configure<BucketOptions>(builder.Configuration.GetSection(bucketOptionsSectionName));
    }
}