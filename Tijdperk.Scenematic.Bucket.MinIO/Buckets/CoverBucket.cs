using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Scenematic.Bucket.Buckets;
using Tijdperk.Scenematic.Bucket.Models;
using Tijdperk.Scenematic.Bucket.Models.Covers;
using Tijdperk.Scenematic.Bucket.Options;

namespace Tijdperk.Scenematic.Bucket.MinIO.Buckets;

public partial class CoverBucket(
    IMinioClient minioClient,
    IOptions<BucketOptions> bucketOptions,
    ILogger<CoverBucket> logger
) : ICoverBucket
{
    [GeneratedRegex("^[a-z0-9-/]{3,63}$")]
    private static partial Regex BucketNameRegex();

    [GeneratedRegex("^[a-z0-9-/_.]+$")]
    private static partial Regex ObjectNameRegex();

    public async Task<ResultObject<PutCoverResult>> PutCoverAsync(
        PutCoverArgs args,
        CancellationToken cancellationToken = default
    )
    {
        var coverOptions = bucketOptions.Value.Cover;

        var bucket = coverOptions.Bucket;
        if (!IsBucketNameValid(bucket)) return ResultObject<PutCoverResult>.Validation("Bucket name is invalid.");

        var id = Guid.NewGuid().ToString();

        var objectName = GetPathName(id, args.Group, args.Extension, coverOptions.ObjectNameTemplate);
        if (!IsObjectNameValid(objectName))
            return ResultObject<PutCoverResult>.Validation("Object name is invalid.");

        logger.LogInformation(
            "Uploading object to MinIO. Bucket: {Bucket}, ObjectName: {ObjectName}",
            bucket,
            objectName
        );

        try
        {
            await minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(objectName)
                    .WithFileName(args.LocalPath)
                    .WithContentType(args.ContentType),
                cancellationToken
            );

            logger.LogInformation(
                "Successfully uploaded object to MinIO. Bucket: {Bucket}, ObjectName: {ObjectName}",
                bucket,
                objectName
            );

            return ResultObject<PutCoverResult>.Success(new PutCoverResult(
                Id: id,
                Group: args.Group,
                Extension: args.Extension,
                BucketName: bucket,
                ObjectName: objectName
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to upload object to MinIO. Bucket: {Bucket}, ObjectName: {ObjectName}",
                bucket,
                objectName
            );
            return ResultObject<PutCoverResult>.InternalError("Failed to upload object to MinIO.");
        }
    }

    private static bool IsBucketNameValid(string bucketName)
        => !string.IsNullOrWhiteSpace(bucketName) && BucketNameRegex().IsMatch(bucketName);

    private static bool IsObjectNameValid(string objectName)
        => !string.IsNullOrWhiteSpace(objectName) && ObjectNameRegex().IsMatch(objectName);

    private static string GetPathName(
        string id,
        string group,
        string extension,
        string nameTemplate
    ) => nameTemplate
        .Trim()
        .Replace("{Id}", id)
        .Replace("{Group}", group)
        .Replace("{Extension}", extension);
}