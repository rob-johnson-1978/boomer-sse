using BoomerSse.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace BoomerSse;

public sealed class BoomerSseOptions
{
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

    public BoomerSseOptions UseAuthentication()
    {
        MustUseAuthentication = true;

        return this;
    }

    public BoomerSseOptions AddClientEventHandler<TEvent, TEventHandler>(string eventType)
        where TEvent : class
        where TEventHandler : IHandleClientEvents<TEvent>
    {
        _hostApplicationBuilder.Services.TryAddScoped(typeof(TEventHandler));

        Handling.AddHandler<TEvent, TEventHandler>(eventType);

        return this;
    }

    public BoomerSseOptions AddClientEventHandler<TEventHandler>(string eventType)
        where TEventHandler : IHandleClientEvents<DefaultClientEvent>
    {
        _hostApplicationBuilder.Services.TryAddScoped(typeof(TEventHandler));

        Handling.AddHandler<DefaultClientEvent, TEventHandler>(eventType);

        return this;
    }

    public BoomerSseOptions AddSynchronousClientEventHandler<TEvent, TEventHandler>(string eventType)
        where TEvent : class
        where TEventHandler : ISynchronouslyHandleClientEvents<TEvent>
    {
        _hostApplicationBuilder.Services.TryAddScoped(typeof(TEventHandler));

        Handling.AddSynchronousHandler<TEvent, TEventHandler>(eventType);

        return this;
    }

    public BoomerSseOptions AddSynchronousClientEventHandler<TEventHandler>(string eventType)
        where TEventHandler : ISynchronouslyHandleClientEvents<DefaultClientEvent>
    {
        _hostApplicationBuilder.Services.TryAddScoped(typeof(TEventHandler));

        Handling.AddSynchronousHandler<DefaultClientEvent, TEventHandler>(eventType);

        return this;
    }

    public BoomerSseOptions ScanAssemblyForClientEventHandlers(Assembly assembly)
    {
        // todo: find handlers, both static and non, both async and sync

        return this;
    }

    internal bool MustUseAuthentication { get; private set; }

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
}