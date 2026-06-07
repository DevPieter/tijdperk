using StackExchange.Redis;
using Tijdperk.Scenematic.Queue.Queues;

namespace Tijdperk.Scenematic.Queue.Redis.Queues;

public class GenerateCoverJobQueue(IConnectionMultiplexer redis) : IGenerateCoverJobQueue
{
    private const string QueueKey = "Tijdperk.Scenematic:GenerateCover:Jobs";
    private const string ProcessingQueueKey = "Tijdperk.Scenematic:GenerateCover:Jobs:Processing";

    private readonly IDatabase _database = redis.GetDatabase();

    public async Task EnqueueAsync(Guid jobId) => await _database.ListLeftPushAsync(
        QueueKey,
        jobId.ToString()
    );

    public async Task<Guid?> DequeueAsync(CancellationToken cancellationToken)
    {
        // TODO - Check if there is a better way of doing this
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await _database.ExecuteAsync(
                "BRPOPLPUSH",
                QueueKey,
                ProcessingQueueKey,
                "5" // timeout seconds
            );

            if (result.IsNull) continue;

            var value = (string)result!;
            return Guid.Parse(value);
        }

        throw new OperationCanceledException(cancellationToken);
    }

    public async Task AcknowledgeAsync(Guid jobId) => await _database.ListRemoveAsync(
        ProcessingQueueKey,
        jobId.ToString(),
        1
    );

    // TODO - Periodically move stuck jobs from ProcessingQueueKey back to QueueKey
    // TODO - Implement job timeouts and retries

    // var stuckJobs = await _database.ListRangeAsync(ProcessingQueueKey);
    //
    //     foreach (var job in stuckJobs)
    // {
    //     await _database.ListRemoveAsync(ProcessingQueueKey, job);
    //     await _database.ListLeftPushAsync(QueueKey, job);
    // }
}