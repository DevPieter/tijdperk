using System.IO.Compression;
using System.Text.Json;
using Tijdperk.Scenematic.Bucket.Buckets;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Processors;

public class CoverProcessor(
    HttpClient client,
    Renderer renderer,
    ICoverBucket bucket,
    ILogger<CoverProcessor> logger
)
{
    public enum MusicPackProcessingStage
    {
        Starting,
        Downloading,
        Processing,
        Uploading,
        Completed,
        Failed
    }

    public class MusicPackProcessingLockFile
    {
        public Guid JobId { get; set; }
        public Guid PackId { get; set; }
        public MusicPackProcessingStage Stage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    private bool CreateFreshLockFile(Guid jobId, Guid packId)
    {
        var lockFilePath = Path.Combine(GetWorkDirPath(jobId), "lock.json");
        if (File.Exists(lockFilePath))
        {
            logger.LogWarning("Lock file for job {JobId} already exists. Skipping creation.", jobId);
            return false;
        }

        var lockFile = new MusicPackProcessingLockFile()
        {
            JobId = jobId,
            PackId = packId,
            Stage = MusicPackProcessingStage.Starting,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var lockFileContent = JsonSerializer.Serialize(lockFile);
        File.WriteAllText(lockFilePath, lockFileContent);
        return true;
    }

    private bool UpdateLockFileStage(Guid jobId, MusicPackProcessingStage newStage)
    {
        var lockFilePath = Path.Combine(GetWorkDirPath(jobId), "lock.json");
        if (!File.Exists(lockFilePath))
        {
            logger.LogWarning("Lock file for job {JobId} does not exist. Cannot update stage.", jobId);
            return false;
        }

        var lockFileContent = File.ReadAllText(lockFilePath);
        var lockFile = JsonSerializer.Deserialize<MusicPackProcessingLockFile>(lockFileContent);
        if (lockFile is null)
        {
            logger.LogError("Failed to deserialize lock file for job {JobId}. Cannot update stage.", jobId);
            return false;
        }

        lockFile.Stage = newStage;
        lockFile.UpdatedAt = DateTime.UtcNow;

        lockFileContent = JsonSerializer.Serialize(lockFile);
        File.WriteAllText(lockFilePath, lockFileContent);
        return true;
    }

    private string GetWorkDirPath(Guid jobId)
        => Path.Combine(Path.GetTempPath(), "MusicPackWorker", jobId.ToString());

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

    private string CreateDir(Guid jobId, string dirName)
    {
        var path = Path.Combine(GetWorkDirPath(jobId), dirName);
        Directory.CreateDirectory(path);

        return path;
    }

    private void DeleteWorkDir(Guid jobId)
    {
        var path = GetWorkDirPath(jobId);
        if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
    }
}