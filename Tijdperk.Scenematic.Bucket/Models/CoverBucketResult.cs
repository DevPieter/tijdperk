namespace Tijdperk.Scenematic.Bucket.Models;

public record CoverBucketResult(
    string Id,
    string Group,
    string Extension,
    string BucketName,
    string ObjectName
)
{
    public string Path => $"{BucketName}/{ObjectName}";

    public override string ToString()
    {
        return
            $"{nameof(Path)}: {Path}, {nameof(Id)}: {Id}, {nameof(Group)}: {Group}, {nameof(Extension)}: {Extension}, {nameof(BucketName)}: {BucketName}, {nameof(ObjectName)}: {ObjectName}";
    }
}