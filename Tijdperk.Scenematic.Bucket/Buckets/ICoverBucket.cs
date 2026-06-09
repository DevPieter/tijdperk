using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Scenematic.Bucket.Models;

namespace Tijdperk.Scenematic.Bucket.Buckets;

public interface ICoverBucket
{
    Task<ResultObject<CoverBucketResult>> PutObjectAsync(
        string group,
        string extension,
        string localPath,
        string contentType,
        CancellationToken cancellationToken = default
    );
}