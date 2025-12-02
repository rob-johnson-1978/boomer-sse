using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BoomerSse;

public static class Bootstrapping
{
    public static WebApplicationBuilder UseBoomerSse(this WebApplicationBuilder builder, Action<BoomerSseOptions> configure)
    {
        var options = new BoomerSseOptions(builder);

        configure(options);

        options.Validate();
        
        builder.Services.AddSingleton(options);

        builder.WebHost.UseStaticWebAssets();

        return builder;
    }
    
    public static WebApplication UseBoomerSse(this WebApplication app)
    {
        app
            .MapPost("/bsse/pub", RequestHandlers.Pub)
            .RequireAuthorization();

        app
            .MapGet("/bsse/sub", RequestHandlers.Sub)
            .RequireAuthorization();

        return app;
    }
}