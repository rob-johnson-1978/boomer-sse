using System.Collections.Immutable;

namespace BoomerSse.Abstractions;

public interface IReceiveServerEvents
{
    Task ReceiveServerEvents(
        Guid sessionId, 
        ImmutableArray<ServerEventBody> serverEventBodies, 
        CancellationToken cancellationToken
    );

    Task SubscribeToServerEvents(
        Guid sessionId, 
        Func<ServerEventBody, Task> onServerEventReceived, 
        CancellationToken cancellationToken
    );
}