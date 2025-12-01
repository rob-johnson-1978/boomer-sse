using BoomerSse.Abstractions;

namespace BoomerSse.Demo;

internal sealed class SomethingClassicHappenedEventHandlerHandler : ClientEventHandler<SomethingHappened>
{
    protected override Task<ServerEventBody> Handle(SomethingHappened @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

internal sealed class SomethingClassicSyncHappenedEventHandler : SynchronousClientEventHandler<SomethingSyncHappened>
{
    protected override ServerEventBody Handle(SomethingSyncHappened @event)
    {
        throw new NotImplementedException();
    }
}