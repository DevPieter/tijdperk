using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Scenematic.Bucket.Models;
using Tijdperk.Scenematic.Bucket.Models.Covers;

namespace Tijdperk.Scenematic.Bucket.Buckets;

public interface ICoverBucket
{
    Task<ResultObject<PutCoverResult>> PutCoverAsync(PutCoverArgs args, CancellationToken cancellationToken = default);
}