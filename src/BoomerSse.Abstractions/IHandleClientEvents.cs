namespace BoomerSse.Abstractions;

public interface IHandleClientEvents<in TEvent>
    where TEvent: class
{
    Task<ServerEventBody> Handle(TEvent @event, CancellationToken cancellationToken);
}

public interface IHandleClientEventsSynchronously<in TEvent>
    where TEvent: class
{
    ServerEventBody Handle(TEvent @event);
}