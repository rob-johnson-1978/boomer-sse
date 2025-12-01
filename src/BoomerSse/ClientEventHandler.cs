using BoomerSse.Abstractions;

namespace BoomerSse;

public abstract class ClientEventHandler<TEvent>
    where TEvent: class
{
    protected abstract Task<ServerEventBody> Handle(TEvent @event, CancellationToken cancellationToken);

    internal async Task<ServerEventBody> HandleInternal(TEvent @event, CancellationToken cancellationToken) =>
        await Handle(@event, cancellationToken);
}

public abstract class SynchronousClientEventHandler<TEvent>
    where TEvent: class
{
    protected abstract ServerEventBody Handle(TEvent @event);

    internal ServerEventBody HandleInternal(TEvent @event) =>
        Handle(@event);
}