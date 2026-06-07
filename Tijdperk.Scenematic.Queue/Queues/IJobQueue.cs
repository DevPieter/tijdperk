namespace Tijdperk.Scenematic.Queue.Queues;

public interface IJobQueue
{
    Task EnqueueAsync(Guid jobId);

    Task<Guid?> DequeueAsync(CancellationToken cancellationToken);

    Task AcknowledgeAsync(Guid jobId);
}