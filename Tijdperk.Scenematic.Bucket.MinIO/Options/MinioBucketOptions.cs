namespace Tijdperk.Scenematic.Bucket.MinIO.Options;

public class MinioBucketOptions
{
    public string Endpoint { get; init; } = null!;
    public string AccessKey { get; init; } = null!;
    public string SecretKey { get; init; } = null!;
}