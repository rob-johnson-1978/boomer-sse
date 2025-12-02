using BoomerSse.Abstractions;

namespace BoomerSse;

public interface IHandleClientEvents<in TEvent>
    where TEvent: class
{
    Task<ServerEventBody> Handle(TEvent @event, CancellationToken cancellationToken);
}

public interface ISynchronouslyHandleClientEvents<in TEvent>
    where TEvent: class
{
    ServerEventBody Handle(TEvent @event);
}