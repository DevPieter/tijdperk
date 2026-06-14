namespace Tijdperk.Scenematic.Common.Enums;

public enum GenerateCoverJobFailureReason
{
    None,
    Unknown,
    DownloadingFailed,
    RenderingFailed,
    WritingFailed,
    UploadingFailed,
    InternalError
}