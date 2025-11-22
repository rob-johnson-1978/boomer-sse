using BoomerSse.Abstractions;
using Microsoft.Extensions.Hosting;

namespace BoomerSse;

public class BoomerSseOptions
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
            .SingleOrDefault(x=>x.ServiceType == typeof(IReceiveClientEvents));

        if (clientEventReceiver == null)
        {
            throw new InvalidOperationException(
                $"Strategy '{_scaleOutStrategy.GetType()}' " +
                $"does not register {nameof(IReceiveClientEvents)} in the container " +
                $"during it's {nameof(IDecideScaleOutStrategy.Configure)} method"
            );
        }
    }
}