namespace Tijdperk.Scenematic.Bucket.Models.Covers;

public record PutCoverResult(
    string Id,
    string Group,
    string Extension,
    string BucketName,
    string ObjectName
)
{
    public string Path => $"{BucketName}/{ObjectName}";
}