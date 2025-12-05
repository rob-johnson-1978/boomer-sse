using System.Collections.Concurrent;
using System.Collections.Immutable;
using BoomerSse.Abstractions;

namespace BoomerSse.InMemory;

public class ClientEventManager : IReceiveClientEvents, IReceiveServerEvents
{
    private readonly ConcurrentDictionary<Guid, Func<ClientEventBody, Task>> _clientEventHandlers = new();
    private readonly ConcurrentDictionary<Guid, Func<ServerEventBody, Task>> _serverEventHandlers = new();

    static ClientEventManager()
    {
    }

    private ClientEventManager()
    {
    }

    public static ClientEventManager Instance { get; } = new();
    
    public async Task ReceiveClientEvent(Guid sessionId, ClientEventBody clientEventBody, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (_clientEventHandlers.TryGetValue(sessionId, out var handler))
        {
            await handler(clientEventBody);
        }
    }

    public async Task SubscribeToClientEvents(Guid sessionId, Func<ClientEventBody, Task> onClientEventReceived, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!_clientEventHandlers.TryGetValue(sessionId, out _))
        {
            _clientEventHandlers.TryAdd(sessionId, onClientEventReceived);
        }
    }

    public async Task StopSubscribingToClientEvents(Guid sessionId)
    {
        await Task.CompletedTask;

        _clientEventHandlers.TryRemove(sessionId, out _);
    }

    public async Task ReceiveServerEvents(Guid sessionId, ImmutableArray<ServerEventBody> serverEventBodies, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (_serverEventHandlers.TryGetValue(sessionId, out var handler))
        {
            foreach (var serverEventBody in serverEventBodies)
            {
                await handler(serverEventBody);
            }
        }
    }

    public async Task SubscribeToServerEvents(Guid sessionId, Func<ServerEventBody, Task> onServerEventReceived, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!_serverEventHandlers.TryGetValue(sessionId, out _))
        {
            _serverEventHandlers.TryAdd(sessionId, onServerEventReceived);
        }
    }

    public async Task StopSubscribingToServerEvents(Guid sessionId)
    {
        await Task.CompletedTask;

        _serverEventHandlers.TryRemove(sessionId, out _);
    }
}