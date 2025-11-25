using System.Collections.Immutable;
using BoomerSse.Abstractions;

namespace BoomerSse.InMemory;

public class ClientEventManager : IReceiveClientEvents, IReceiveServerEvents
{
    static ClientEventManager()
    {
    }

    private ClientEventManager()
    {
    }

    public static ClientEventManager Instance { get; } = new();
    
    public Task ReceiveClientEvent(Guid sessionId, ClientEventBody clientEventBody, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SubscribeToClientEvents(Guid sessionId, Func<ClientEventBody, Task> onClientEventReceived, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ReceiveServerEvents(Guid sessionId, ImmutableArray<ServerEventBody> serverEventBodies, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SubscribeToServerEvents(Guid sessionId, Func<ServerEventBody, Task> onServerEventReceived, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}