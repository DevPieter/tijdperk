namespace Tijdperk.Scenematic.Bucket.Models.Covers;

public record PutCoverArgs(
    string Group,
    string Extension,
    string LocalPath,
    string ContentType
);