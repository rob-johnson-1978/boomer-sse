namespace BoomerSse.Abstractions;

public interface IReceiveClientEvents
{
    Task Receive(Guid sessionId, ClientEventBody clientEventBody, CancellationToken cancellationToken);
}