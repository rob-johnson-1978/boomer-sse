using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoomerSse;

public static class Bootstrapping
{
    public static IHostApplicationBuilder UseBoomerSse(this IHostApplicationBuilder builder, Action<BoomerSseOptions> configure)
    {
        var options = new BoomerSseOptions(builder);

        configure(options);

        options.Validate();
        
        builder.Services.AddSingleton(options);

        return builder;
    }
    
    public static WebApplication UseBoomerSse(this WebApplication app)
    {
        app
            .MapPost("/bsse/receive-client-event", RequestHandlers.ReceiveClientEvent)
            .RequireAuthorization();

        return app;
    }
}