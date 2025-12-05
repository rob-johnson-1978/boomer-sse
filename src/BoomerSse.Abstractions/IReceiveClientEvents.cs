namespace BoomerSse.Abstractions;

public interface IReceiveClientEvents
{
    Task ReceiveClientEvent(
        Guid sessionId, 
        ClientEventBody clientEventBody, 
        CancellationToken cancellationToken
    );

    Task SubscribeToClientEvents(
        Guid sessionId, 
        Func<ClientEventBody, Task> onClientEventReceived, 
        CancellationToken cancellationToken
    );

    Task StopSubscribingToClientEvents(Guid sessionId);
}