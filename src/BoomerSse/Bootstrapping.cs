using Microsoft.AspNetCore.Builder;

namespace BoomerSse;

public static class Bootstrapping
{
    public static WebApplication UseBoomerSse(this WebApplication app)
    {
        app
            .MapPost("/bsse/receive-client-event", RequestHandlers.ReceiveClientEvent)
            .RequireAuthorization();

        return app;
    }
}