using BoomerSse.Abstractions;

namespace BoomerSse.Demo.Pages.Home;

internal sealed class HomeReadyHandler(IProvideRazorFunctionality razorFunctionality) : IHandleClientEvents<DefaultClientEvent>
{
    public async Task<ServerEventBody> Handle(DefaultClientEvent @event, CancellationToken cancellationToken) =>
        await razorFunctionality.RenderPartial(
            "/Pages/Home/_HomeReadyPartial.cshtml", 
            RenderAction.Insert
        );
    
}