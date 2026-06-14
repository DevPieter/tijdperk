using System.Text.Json;
using SkiaSharp;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Scenematic.Bucket.Buckets;
using Tijdperk.Scenematic.Bucket.Models.Covers;
using Tijdperk.Scenematic.Common.Enums;
using Tijdperk.Scenematic.Worker.CoverGenerator.Enums;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models.Covers;
using Tijdperk.Scenematic.Worker.CoverGenerator.Rendering.Overlays;
using Tijdperk.Scenematic.Worker.CoverGenerator.Rendering.Scenes;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Processors;

public class CoverProcessor(
    HttpClient client,
    ICoverBucket bucket,
    ILogger<CoverProcessor> logger
)
{
    private const int Fps = 16;
    private const int Width = 1200;
    private const int Height = 450;

    private static async Task<SKImage?> TryDownloadImage(HttpClient client, string url)
    {
        try
        {
            var bytes = await client.GetByteArrayAsync(url);
            return SKImage.FromEncodedData(bytes);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ProcessCoverResult> ProcessAsync(ProcessCoverArgs args, CancellationToken cancellationToken)
    {
        var workDirPath = CreateFreshWorkDir(args.JobId);

        string bucketMainCoverPath;
        string bucketDebounceCoverPath;
        string bucketProcessingCoverPath;

        var lockFileCreated = CreateFreshLockFile(args.JobId);
        if (!lockFileCreated)
        {
            logger.LogError(
                "Failed to create lock file for job {JobId}. Aborting processing.",
                args.JobId
            );

            return ProcessCoverResult.Failure(
                GenerateCoverJobFailureReason.InternalError,
                "Failed to create lock file."
            );
        }

        UpdateLockFileStage(args.JobId, ProcessCoverStage.Initializing);

        var coverImage = await TryDownloadImage(client, args.AlbumCoverImageUrl);
        if (coverImage is null)
        {
            logger.LogError(
                "Failed to download cover image for job {JobId} from URL {CoverUrl}. Aborting processing.",
                args.JobId,
                args.AlbumCoverImageUrl
            );

            UpdateLockFileStage(
                args.JobId,
                ProcessCoverStage.Failed,
                "Failed to download cover image."
            );

            return ProcessCoverResult.Failure(
                GenerateCoverJobFailureReason.DownloadingFailed,
                "Failed to download cover image."
            );
        }

        var avatarImages = new Dictionary<string, SKImage>();

        foreach (var vote in args.CoverVotes)
        {
            if (avatarImages.ContainsKey(vote.AvatarUrl)) continue;

            var avatarImage = await TryDownloadImage(client, vote.AvatarUrl);
            if (avatarImage is not null) avatarImages[vote.AvatarUrl] = avatarImage;
        }

        try
        {
            var renderMainCoverTask = new Task<byte[]>(() =>
            {
                using var renderer = new Renderer(Fps, Width, Height);
                var cover = new CoverSceneRenderer(
                    args.AlbumTitle,
                    args.AlbumArtist,
                    args.CoverVotes,
                    coverImage,
                    avatarImages
                    /*, TODO - Specify type*/
                );

                renderer.Add(
                    new Scene(
                        "Main Cover",
                        0,
                        cover
                    ),
                    new Scene(
                        "Main Cover",
                        4,
                        cover
                    ),
                    new Scene(
                        "Main Cover",
                        6,
                        cover,
                        1
                    ).AddOverlay(new BrandingOverlay())
                );

                renderer.Initialize();
                return renderer.Render();
            });

            var renderDebounceCoverTask = new Task<byte[]>(() =>
            {
                using var renderer = new Renderer(Fps, Width, Height);
                var cover = new CoverSceneRenderer(
                    args.AlbumTitle,
                    args.AlbumArtist,
                    args.CoverVotes,
                    coverImage,
                    avatarImages
                    /*, TODO - Specify type*/
                );

                renderer.Add(
                    new Scene(
                        "Debounce Cover",
                        0,
                        cover
                    ),
                    new Scene(
                        "Debounce Cover",
                        4,
                        cover
                    )
                );

                renderer.Initialize();
                return renderer.Render();
            });

            var renderProcessingCoverTask = new Task<byte[]>(() =>
            {
                using var renderer = new Renderer(Fps, Width, Height);
                var cover = new CoverSceneRenderer(
                    args.AlbumTitle,
                    args.AlbumArtist,
                    args.CoverVotes,
                    coverImage,
                    avatarImages
                    /*, TODO - Specify type*/
                );

                renderer.Add(
                    new Scene(
                        "Processing Cover",
                        0,
                        cover
                    ),
                    new Scene(
                        "Processing Cover",
                        4,
                        cover
                    )
                );

                renderer.Initialize();
                return renderer.Render();
            });

            UpdateLockFileStage(args.JobId, ProcessCoverStage.Rendering);

            try
            {
                renderMainCoverTask.Start();
                renderDebounceCoverTask.Start();
                renderProcessingCoverTask.Start();

                await Task.WhenAll(
                    renderMainCoverTask,
                    renderDebounceCoverTask,
                    renderProcessingCoverTask
                );

                coverImage.Dispose();
                foreach (var avatarImage in avatarImages.Values) avatarImage.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to render covers for job {JobId}.", args.JobId);
                UpdateLockFileStage(args.JobId, ProcessCoverStage.Failed, ex.Message);

                return ProcessCoverResult.Failure(
                    GenerateCoverJobFailureReason.RenderingFailed,
                    "Failed to render covers."
                );
            }

            UpdateLockFileStage(args.JobId, ProcessCoverStage.Writing);

            var localMainCoverPath = Path.Combine(workDirPath, "main_cover.webp");
            var localDebounceCoverPath = Path.Combine(workDirPath, "debounce_cover.webp");
            var localProcessingCoverPath = Path.Combine(workDirPath, "processing_cover.webp");

            try
            {
                await File.WriteAllBytesAsync(
                    localMainCoverPath,
                    renderMainCoverTask.Result,
                    cancellationToken
                );
                await File.WriteAllBytesAsync(
                    localDebounceCoverPath,
                    renderDebounceCoverTask.Result,
                    cancellationToken
                );
                await File.WriteAllBytesAsync(
                    localProcessingCoverPath,
                    renderProcessingCoverTask.Result,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to write cover files for job {JobId}.", args.JobId);
                UpdateLockFileStage(args.JobId, ProcessCoverStage.Failed, ex.Message);

                return ProcessCoverResult.Failure(
                    GenerateCoverJobFailureReason.WritingFailed,
                    "Failed to write cover files."
                );
            }

            UpdateLockFileStage(args.JobId, ProcessCoverStage.Uploading);

            try
            {
                var uploadMainCoverTask = bucket.PutCoverAsync(new PutCoverArgs(
                        Group: $"{args.JobId}",
                        Extension: "webp",
                        LocalPath: localMainCoverPath,
                        ContentType: "image/webp"
                    ), cancellationToken
                );

                var uploadDebounceCoverTask = bucket.PutCoverAsync(new PutCoverArgs(
                        Group: $"{args.JobId}",
                        Extension: "webp",
                        LocalPath: localDebounceCoverPath,
                        ContentType: "image/webp"
                    ), cancellationToken
                );

                var uploadProcessingCoverTask = bucket.PutCoverAsync(new PutCoverArgs(
                        Group: $"{args.JobId}",
                        Extension: "webp",
                        LocalPath: localProcessingCoverPath,
                        ContentType: "image/webp"
                    ), cancellationToken
                );

                await Task.WhenAll(
                    uploadMainCoverTask,
                    uploadDebounceCoverTask,
                    uploadProcessingCoverTask
                );

                var mainUploadResult = uploadMainCoverTask.Result;
                var debounceUploadResult = uploadDebounceCoverTask.Result;
                var processUploadCoverResult = uploadProcessingCoverTask.Result;

                if (mainUploadResult.IsError)
                {
                    logger.LogError(
                        "Failed to upload main cover for job {JobId}. Error: {Error}",
                        args.JobId,
                        mainUploadResult.Message
                    );
                    UpdateLockFileStage(
                        args.JobId,
                        ProcessCoverStage.Failed,
                        $"Failed to upload main cover: {mainUploadResult.Message}"
                    );

                    return ProcessCoverResult.Failure(
                        GenerateCoverJobFailureReason.UploadingFailed,
                        $"Failed to upload main cover: {mainUploadResult.Message}"
                    );
                }

                if (mainUploadResult.Data is null || string.IsNullOrEmpty(mainUploadResult.Data.Path))
                {
                    logger.LogError(
                        "Main cover upload for job {JobId} did not return a valid path.",
                        args.JobId
                    );
                    UpdateLockFileStage(
                        args.JobId,
                        ProcessCoverStage.Failed,
                        "Main cover upload did not return a valid path."
                    );

                    return ProcessCoverResult.Failure(
                        GenerateCoverJobFailureReason.UploadingFailed,
                        "Main cover upload did not return a valid path."
                    );
                }

                if (debounceUploadResult.IsError)
                {
                    logger.LogError(
                        "Failed to upload debounce cover for job {JobId}. Error: {Error}",
                        args.JobId,
                        debounceUploadResult.Message
                    );
                    UpdateLockFileStage(
                        args.JobId,
                        ProcessCoverStage.Failed,
                        $"Failed to upload debounce cover: {debounceUploadResult.Message}"
                    );

                    return ProcessCoverResult.Failure(
                        GenerateCoverJobFailureReason.UploadingFailed,
                        $"Failed to upload debounce cover: {debounceUploadResult.Message}"
                    );
                }

                if (debounceUploadResult.Data is null || string.IsNullOrEmpty(debounceUploadResult.Data.Path))
                {
                    logger.LogError(
                        "Debounce cover upload for job {JobId} did not return a valid path.",
                        args.JobId
                    );
                    UpdateLockFileStage(
                        args.JobId,
                        ProcessCoverStage.Failed,
                        "Debounce cover upload did not return a valid path."
                    );

                    return ProcessCoverResult.Failure(
                        GenerateCoverJobFailureReason.UploadingFailed,
                        "Debounce cover upload did not return a valid path."
                    );
                }

                if (processUploadCoverResult.IsError)
                {
                    logger.LogError(
                        "Failed to upload processing cover for job {JobId}. Error: {Error}",
                        args.JobId,
                        processUploadCoverResult.Message
                    );
                    UpdateLockFileStage(
                        args.JobId,
                        ProcessCoverStage.Failed,
                        $"Failed to upload processing cover: {processUploadCoverResult.Message}"
                    );

                    return ProcessCoverResult.Failure(
                        GenerateCoverJobFailureReason.UploadingFailed,
                        $"Failed to upload processing cover: {processUploadCoverResult.Message}"
                    );
                }

                if (processUploadCoverResult.Data is null || string.IsNullOrEmpty(processUploadCoverResult.Data.Path))
                {
                    logger.LogError(
                        "Processing cover upload for job {JobId} did not return a valid path.",
                        args.JobId
                    );
                    UpdateLockFileStage(
                        args.JobId,
                        ProcessCoverStage.Failed,
                        "Processing cover upload did not return a valid path."
                    );

                    return ProcessCoverResult.Failure(
                        GenerateCoverJobFailureReason.UploadingFailed,
                        "Processing cover upload did not return a valid path."
                    );
                }

                bucketMainCoverPath = mainUploadResult.Data.Path;
                bucketDebounceCoverPath = debounceUploadResult.Data.Path;
                bucketProcessingCoverPath = processUploadCoverResult.Data.Path;

                logger.LogInformation("Successfully uploaded covers for job {JobId}.", args.JobId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upload covers for job {JobId}.", args.JobId);
                UpdateLockFileStage(args.JobId, ProcessCoverStage.Failed, ex.Message);

                return ProcessCoverResult.Failure(
                    GenerateCoverJobFailureReason.UploadingFailed,
                    "Failed to upload covers."
                );
            }

            UpdateLockFileStage(args.JobId, ProcessCoverStage.Verifying);
            // TODO - Verify

            UpdateLockFileStage(args.JobId, ProcessCoverStage.Completed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload covers for job {JobId}.", args.JobId);

            // TODO - This file will get deleted immediately after, consider if we want to keep it around for debugging purposes
            UpdateLockFileStage(args.JobId, ProcessCoverStage.Failed, ex.Message);

            return ProcessCoverResult.Failure(
                GenerateCoverJobFailureReason.Unknown,
                "An unknown error occurred during processing."
            );
        }
        finally
        {
            DeleteWorkDir(args.JobId);

            if (DoesWorkDirExist(args.JobId))
            {
                logger.LogError(
                    "Work directory for job {JobId} still exists after processing.",
                    args.JobId
                );
            }
            else
            {
                logger.LogInformation(
                    "Work directory for job {JobId} successfully deleted after processing.",
                    args.JobId
                );
            }
        }

        return ProcessCoverResult.Success(
            mainCoverPath: bucketMainCoverPath,
            debounceCoverPath: bucketDebounceCoverPath,
            processingCoverPath: bucketProcessingCoverPath
        );
    }

    private bool CreateFreshLockFile(Guid jobId)
    {
        var lockFilePath = Path.Combine(GetWorkDirPath(jobId), "lock.json");
        if (File.Exists(lockFilePath))
        {
            logger.LogWarning("Lock file for job {JobId} already exists. Skipping creation.", jobId);
            return false;
        }

        var lockFile = new ProcessCoverLock()
        {
            JobId = jobId,
            Stage = ProcessCoverStage.Starting,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var lockFileContent = JsonSerializer.Serialize(lockFile);
        File.WriteAllText(lockFilePath, lockFileContent);
        return true;
    }

    private bool UpdateLockFileStage(Guid jobId, ProcessCoverStage newStage, string? extraInfo = null)
    {
        var lockFilePath = Path.Combine(GetWorkDirPath(jobId), "lock.json");
        if (!File.Exists(lockFilePath))
        {
            logger.LogWarning("Lock file for job {JobId} does not exist. Cannot update stage.", jobId);
            return false;
        }

        var lockFileContent = File.ReadAllText(lockFilePath);
        var lockFile = JsonSerializer.Deserialize<ProcessCoverLock>(lockFileContent);
        if (lockFile is null)
        {
            logger.LogError("Failed to deserialize lock file for job {JobId}. Cannot update stage.", jobId);
            return false;
        }

        lockFile.Stage = newStage;

        if (!string.IsNullOrEmpty(extraInfo))
        {
            lockFile.ExtraInfo = string.IsNullOrEmpty(lockFile.ExtraInfo)
                ? extraInfo
                : $"{lockFile.ExtraInfo}; {extraInfo}";
        }

        lockFile.UpdatedAt = DateTime.UtcNow;

        lockFileContent = JsonSerializer.Serialize(lockFile);
        File.WriteAllText(lockFilePath, lockFileContent);
        return true;
    }

    private string GetWorkDirPath(Guid jobId)
        => Path.Combine(Path.GetTempPath(), "tijdperk/Tijdperk.Scenematic.Worker.CoverGenerator", jobId.ToString());

    private string CreateFreshWorkDir(Guid jobId)
    {
        var path = GetWorkDirPath(jobId);
        if (Directory.Exists(path))
        {
            logger.LogWarning("Work directory for job {JobId} already exists. Deleting it.", jobId);
            DeleteWorkDir(jobId);
        }

        Directory.CreateDirectory(path);
        return path;
    }

    private void DeleteWorkDir(Guid jobId)
    {
        var path = GetWorkDirPath(jobId);
        if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
    }

    private bool DoesWorkDirExist(Guid jobId)
        => Directory.Exists(GetWorkDirPath(jobId));
}