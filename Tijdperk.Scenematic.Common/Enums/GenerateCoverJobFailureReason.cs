namespace Tijdperk.Scenematic.Common.Enums;

public enum GenerateCoverJobFailureReason
{
    None,
    Unknown,
    NotFound,
    DownloadingFailed,
    RenderingFailed,
    WritingFailed,
    UploadingFailed,
    InternalError
}