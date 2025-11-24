namespace BoomerSse.Abstractions;

public interface IReceiveClientEvents
{
    Task Receive(Guid sessionId, ClientEventBody clientEventBody, CancellationToken cancellationToken);

    Task SubscribeToReceivedEvents(Guid sessionId, CancellationToken cancellationToken);
}