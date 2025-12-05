using BoomerSse.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.Json;

namespace BoomerSse;

internal sealed class Handling
{
    private static readonly Dictionary<string, List<Func<IServiceProvider, ClientEventBody, CancellationToken, Task<ServerEventBody>>>>
        ClientEventHandlers = [];

    private static readonly Dictionary<string, List<Func<IServiceProvider, ClientEventBody, ServerEventBody>>>
        SynchronousClientEventHandlers = [];

    internal static void AddHandler<TEvent, TEventHandler>(string eventType)
        where TEvent : class
        where TEventHandler : IHandleClientEvents<TEvent>
    {
        if (!ClientEventHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers = [];
            ClientEventHandlers[eventType] = handlers;
        }

        handlers.Add(BuildClientEventHandlingTaskFunc<TEvent, TEventHandler>());
    }

    internal static void AddSynchronousHandler<TEvent, TEventHandler>(string eventType)
        where TEvent : class
        where TEventHandler : ISynchronouslyHandleClientEvents<TEvent>
    {
        if (!SynchronousClientEventHandlers.TryGetValue(eventType, out var syncHandlers))
        {
            syncHandlers = [];
            SynchronousClientEventHandlers[eventType] = syncHandlers;
        }

        syncHandlers.Add(BuildSynchronousClientEventHandlingTaskFunc<TEvent, TEventHandler>());
    }

    internal static async Task<ImmutableArray<ServerEventBody>> BuildClientEventHandlingTask(
        ClientEventBody clientEventBody,
        IServiceProvider scopedServiceProvider,
        CancellationToken cancellationToken)
    {
        if (!ClientEventHandlers.TryGetValue(clientEventBody.Event, out var handlers))
        {
            handlers = [];
        }

        if (!SynchronousClientEventHandlers.TryGetValue(clientEventBody.Event, out var syncHandlers))
        {
            syncHandlers = [];
        }

        if (handlers.Count + syncHandlers.Count < 1)
        {
            return [];
        }

        var tasks = handlers
            .Select(handler => handler(scopedServiceProvider, clientEventBody, cancellationToken))
            .Concat(
                syncHandlers.Select(syncHandler =>
                    Task.Run(() => syncHandler(scopedServiceProvider, clientEventBody), cancellationToken)
                )
            );

        return [.. await Task.WhenAll(tasks)];
    }

    private static Func<IServiceProvider, ClientEventBody, CancellationToken, Task<ServerEventBody>>
        BuildClientEventHandlingTaskFunc<TEvent, TEventHandler>()
        where TEvent : class
        where TEventHandler : IHandleClientEvents<TEvent> =>
        async (sp, clientEventBody, cancellationToken) =>
        {
            await using var newScope = sp.CreateAsyncScope();
            
            var serviceProvider = newScope.ServiceProvider;

            var logger = serviceProvider.GetRequiredService<ILogger<BoomerSseOptions>>();

            var handler = serviceProvider.GetService<TEventHandler>();

            if (handler == null)
            {
                logger.LogWarning(
                    "There is no handler for type {type}; this should not be possible. EventName = {ev}",
                    typeof(TEventHandler),
                    clientEventBody.Event
                );

                return ServerEventBody.Default;
            }

            try
            {
                var @event = BuildClientEvent<TEvent>(clientEventBody);

                return @event == null
                    ? ServerEventBody.Default
                    : await handler.Handle(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An exception was thrown during message handling for event {ev}",
                    clientEventBody.Event
                );

                return ServerEventBody.Default;
            }
        };

    private static Func<IServiceProvider, ClientEventBody, ServerEventBody>
        BuildSynchronousClientEventHandlingTaskFunc<TEvent, TEventHandler>()
        where TEvent : class
        where TEventHandler : ISynchronouslyHandleClientEvents<TEvent> =>
        (sp, clientEventBody) =>
        {
            using var newScope = sp.CreateAsyncScope();

            var serviceProvider = newScope.ServiceProvider;

            var logger = serviceProvider.GetRequiredService<ILogger<BoomerSseOptions>>();

            var handler = serviceProvider.GetService<TEventHandler>();

            if (handler == null)
            {
                logger.LogWarning(
                    "There is no handler for type {type}; this should not be possible. EventName = {ev}",
                    typeof(TEventHandler),
                    clientEventBody.Event
                );

                return ServerEventBody.Default;
            }

            try
            {
                var @event = BuildClientEvent<TEvent>(clientEventBody);

                return @event == null
                    ? ServerEventBody.Default
                    : handler.Handle(@event);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An exception was thrown during message handling for event {ev}",
                    clientEventBody.Event
                );

                return ServerEventBody.Default;
            }
        };

    private static TEvent? BuildClientEvent<TEvent>(ClientEventBody clientEventBody)
        where TEvent : class => typeof(TEvent) switch
        {
            var t when t == typeof(DefaultClientEvent) =>
                new DefaultClientEvent(clientEventBody.Message ?? "") as TEvent
                ?? throw new Exception($"Failed to construct {typeof(TEvent)}"),

            _ when clientEventBody.Data != null =>
                clientEventBody.Data.Deserialize<TEvent>()
                ?? throw new Exception($"Failed to deserialize client event data. Type was {typeof(TEvent)}"),

            _ => null
        };
}