using BoomerSse.Abstractions;

namespace BoomerSse.Demo;


internal sealed class MainLoadedHandler(ILogger<MainLoadedHandler> logger) : IHandleClientEvents<MainLoaded>
{
    public async Task<ServerEventBody> Handle(MainLoaded @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        logger.LogInformation("MainLoaded event received at {ts}", @event.Timestamp);

        return ServerEventBody.Default;
    }
}

internal sealed class HipHappenedHandler(ILogger<HipHappenedHandler> logger) : IHandleClientEvents<DefaultClientEvent>
{
    public async Task<ServerEventBody> Handle(DefaultClientEvent @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        logger.LogInformation("hip_happened event received");

        return ServerEventBody.Default;
    }
}

internal sealed class HipHappenedAgainHandler(ILogger<HipHappenedAgainHandler> logger) : IHandleClientEvents<DefaultClientEvent>
{
    public async Task<ServerEventBody> Handle(DefaultClientEvent @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        logger.LogInformation("hip_happened_again event received, message was '{msg}'", @event.Message);

        return ServerEventBody.Default;
    }
}

internal sealed class MainButtonClickedHandler(ILogger<HipHappenedHandler> logger) : IHandleClientEvents<DefaultClientEvent>
{
    public async Task<ServerEventBody> Handle(DefaultClientEvent @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        logger.LogInformation("main_button_clicked event received");

        return ServerEventBody.Default;
    }
}

internal sealed class MainButtonClickedAgainHandler (ILogger<MainButtonClickedAgainHandler> logger) : IHandleClientEvents<MainButtonClickedAgain>
{
    public async Task<ServerEventBody> Handle(MainButtonClickedAgain @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        
        logger.LogInformation("MainButtonClickedAgain event received at {ts}", @event.Timestamp);
        
        return ServerEventBody.Default;
    }
}

internal sealed class MainFormSubmittedHandler(ILogger<MainFormSubmittedHandler> logger) : IHandleClientEvents<MainFormSubmitted>
{
    public async Task<ServerEventBody> Handle(MainFormSubmitted @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        
        logger.LogInformation(
            "MainFormSubmitted event received with Prop1='{p1}', Prop2='{p2}', Prop3='{p3}'",
            @event.Prop1,
            @event.Prop2,
            @event.Prop3
        );

        return ServerEventBody.Default;
    }
}

internal sealed class MainLoadedHandler2(ILogger<MainLoadedHandler2> logger) : IHandleClientEvents<MainLoaded>
{
    public async Task<ServerEventBody> Handle(MainLoaded @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        logger.LogInformation("[v2] MainLoaded event received at {ts}", @event.Timestamp);

        return ServerEventBody.Default;
    }
}

internal sealed class HipHappenedHandler2(ILogger<HipHappenedHandler2> logger) : IHandleClientEvents<DefaultClientEvent>
{
    public async Task<ServerEventBody> Handle(DefaultClientEvent @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        logger.LogInformation("[v2] hip_happened event received");

        return ServerEventBody.Default;
    }
}

internal sealed class HipHappenedAgainHandler2(ILogger<HipHappenedAgainHandler2> logger) : IHandleClientEvents<DefaultClientEvent>
{
    public async Task<ServerEventBody> Handle(DefaultClientEvent @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        logger.LogInformation("[v2] hip_happened_again event received, message was '{msg}'", @event.Message);

        return ServerEventBody.Default;
    }
}

internal sealed class MainButtonClickedHandler2(ILogger<MainButtonClickedHandler2> logger) : IHandleClientEvents<DefaultClientEvent>
{
    public async Task<ServerEventBody> Handle(DefaultClientEvent @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        logger.LogInformation("[v2] main_button_clicked event received");

        return ServerEventBody.Default;
    }
}

internal sealed class MainButtonClickedAgainHandler2 (ILogger<MainButtonClickedAgainHandler2> logger) : IHandleClientEvents<MainButtonClickedAgain>
{
    public async Task<ServerEventBody> Handle(MainButtonClickedAgain @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        
        logger.LogInformation("[v2] MainButtonClickedAgain event received at {ts}", @event.Timestamp);
        
        return ServerEventBody.Default;
    }
}

internal sealed class MainFormSubmittedHandler2(ILogger<MainFormSubmittedHandler2> logger) : IHandleClientEvents<MainFormSubmitted>
{
    public async Task<ServerEventBody> Handle(MainFormSubmitted @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        
        logger.LogInformation(
            "[v2] MainFormSubmitted event received with Prop1='{p1}', Prop2='{p2}', Prop3='{p3}'",
            @event.Prop1,
            @event.Prop2,
            @event.Prop3
        );

        return ServerEventBody.Default;
    }
}
