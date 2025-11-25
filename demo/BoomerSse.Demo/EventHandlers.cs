using BoomerSse.Abstractions;

namespace BoomerSse.Demo;

internal sealed class SomethingClassicHappenedEventHandler : IHandleClientEvents<SomethingClassicHappened>
{
    public Task<ServerEventBody> Handle(SomethingClassicHappened @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

internal sealed class SomethingClassicSyncHappenedEventHandler : IHandleClientEventsSynchronously<SomethingClassicSyncHappened>
{
    public ServerEventBody Handle(SomethingClassicSyncHappened @event)
    {
        throw new NotImplementedException();
    }
}