using System.Collections.Immutable;
using System.Reflection;
using BoomerSse.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BoomerSse;

public class BoomerSseOptions
{
    private readonly Dictionary<string, List<Func<IServiceProvider, ClientEventBody, CancellationToken, Task<ServerEventBody>>>>
        _clientEventHandlers = [];

    private readonly Dictionary<string, List<Func<IServiceProvider, ClientEventBody, ServerEventBody>>>
        _synchronousClientEventHandlers = [];

    private readonly IHostApplicationBuilder _hostApplicationBuilder;
    private IDecideScaleOutStrategy? _scaleOutStrategy;

    internal BoomerSseOptions(IHostApplicationBuilder hostApplicationBuilder)
    {
        _hostApplicationBuilder = hostApplicationBuilder;
    }

    public BoomerSseOptions UseScaleOutStrategy<TStrategy>(TStrategy strategy)
        where TStrategy : class, IDecideScaleOutStrategy
    {
        if (_scaleOutStrategy != null)
        {
            throw new InvalidOperationException("Strategy already configured");
        }

        strategy.Configure(_hostApplicationBuilder);

        _scaleOutStrategy = strategy;

        return this;
    }

    public BoomerSseOptions AddClientEventHandler<TEvent, TEventHandler>(string eventType)
        where TEvent : class
        where TEventHandler : ClientEventHandler<TEvent>
    {
        _hostApplicationBuilder.Services.TryAddScoped(typeof(TEventHandler));

        if (!_clientEventHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers = [];
            _clientEventHandlers[eventType] = handlers;
        }

        handlers.Add(BuildClientEventHandlingTaskFunc<TEvent, TEventHandler>());

        return this;
    }

    public BoomerSseOptions AddSynchronousClientEventHandler<TEvent, TEventHandler>(string eventType)
        where TEvent : class
        where TEventHandler : SynchronousClientEventHandler<TEvent>
    {
        _hostApplicationBuilder.Services.TryAddScoped(typeof(TEventHandler));

        if(!_synchronousClientEventHandlers.TryGetValue(eventType, out var syncHandlers))
        {
            syncHandlers = [];
            _synchronousClientEventHandlers[eventType] = syncHandlers;
        }

        syncHandlers.Add(BuildSynchronousClientEventHandlingTaskFunc<TEvent, TEventHandler>());

        return this;
    }

    public BoomerSseOptions ScanAssemblyForClientEventHandlers(Assembly assembly)
    {
        // todo: find handlers, both static and non, both async and sync

        return this;
    }

    internal void Validate()
    {
        if (_scaleOutStrategy == null)
        {
            throw new InvalidOperationException(
                "No scale-out strategy has been configured"
            );
        }

        var clientEventReceiver = _hostApplicationBuilder
            .Services
            .SingleOrDefault(x => x.ServiceType == typeof(IReceiveClientEvents));

        if (clientEventReceiver == null)
        {
            throw new InvalidOperationException(
                $"Strategy '{_scaleOutStrategy.GetType()}' " +
                $"does not register {nameof(IReceiveClientEvents)} in the container " +
                $"during its {nameof(IDecideScaleOutStrategy.Configure)} method"
            );
        }

        var serverEventReceiver = _hostApplicationBuilder
            .Services
            .SingleOrDefault(x => x.ServiceType == typeof(IReceiveServerEvents));

        if (serverEventReceiver == null)
        {
            throw new InvalidOperationException(
                $"Strategy '{_scaleOutStrategy.GetType()}' " +
                $"does not register {nameof(IReceiveServerEvents)} in the container " +
                $"during its {nameof(IDecideScaleOutStrategy.Configure)} method"
            );
        }
    }

    internal async Task<ImmutableArray<ServerEventBody>> BuildClientEventHandlingTask(
        ClientEventBody clientEventBody,
        IServiceProvider scopedServiceProvider,
        CancellationToken cancellationToken)
    {
        if (!_clientEventHandlers.TryGetValue(clientEventBody.Event, out var handlers))
        {
            handlers = [];
        }

        if(!_synchronousClientEventHandlers.TryGetValue(clientEventBody.Event, out var syncHandlers))
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
        where TEventHandler : ClientEventHandler<TEvent> =>
        async (sp, clientEventBody, cancellationToken) =>
        {
            var handler = sp.GetService<TEventHandler>();

            if (handler == null)
            {
                // todo: log out
                return ServerEventBody.Default;
            }

            try
            {

                var @event = System.Text.Json.JsonSerializer.Deserialize<TEvent>(
                    clientEventBody.Data ?? "{}"
                );

                if (@event == null)
                {
                    // todo: log out
                    return ServerEventBody.Default;
                }


                return await handler.HandleInternal(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                // todo: logout
                return ServerEventBody.Default;
            }
        };

    private static Func<IServiceProvider, ClientEventBody, ServerEventBody>
        BuildSynchronousClientEventHandlingTaskFunc<TEvent, TEventHandler>()
        where TEvent : class
        where TEventHandler : SynchronousClientEventHandler<TEvent> =>
        (sp, clientEventBody) =>
        {
            var handler = sp.GetService<TEventHandler>();

            if (handler == null)
            {
                // todo: log out
                return ServerEventBody.Default;
            }

            try
            {

                var @event = System.Text.Json.JsonSerializer.Deserialize<TEvent>(
                    clientEventBody.Data ?? "{}"
                );

                if (@event == null)
                {
                    // todo: log out
                    return ServerEventBody.Default;
                }


                return handler.HandleInternal(@event);
            }
            catch (Exception ex)
            {
                // todo: logout
                return ServerEventBody.Default;
            }
        };
}