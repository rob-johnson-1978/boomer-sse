using BoomerSse;
using BoomerSse.Demo;
using BoomerSse.InMemory;
using BoomerSse.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.UseBoomerSse(options =>
{
    switch (Environment.GetEnvironmentVariable("BSSE_SCALEOUT"))
    {
        case "InMemory":
            options
                .UseScaleOutStrategy(new InMemoryScaleOutStrategy())
                .AddClientEventHandler<MainLoaded, MainLoadedHandler>("main_loaded")
                .AddClientEventHandler<HipHappenedHandler>("hip_happened")
                .AddClientEventHandler<HipHappenedAgainHandler>("hip_happened_again")
                .AddClientEventHandler<MainButtonClickedHandler>("main_button_clicked")
                .AddClientEventHandler<MainButtonClickedAgain, MainButtonClickedAgainHandler>("main_button_clicked_again")
                .AddClientEventHandler<MainFormSubmitted, MainFormSubmittedHandler>("main_form_submitted")
                .AddClientEventHandler<MainLoaded, MainLoadedHandler2>("main_loaded")
                .AddClientEventHandler<HipHappenedHandler2>("hip_happened")
                .AddClientEventHandler<HipHappenedAgainHandler2>("hip_happened_again")
                .AddClientEventHandler<MainButtonClickedHandler2>("main_button_clicked")
                .AddClientEventHandler<MainButtonClickedAgain, MainButtonClickedAgainHandler2>("main_button_clicked_again")
                .AddClientEventHandler<MainFormSubmitted, MainFormSubmittedHandler2>("main_form_submitted");
            break;
        case "Redis":
            options.UseScaleOutStrategy(new RedisScaleOutStrategy());
            options.ScanAssemblyForClientEventHandlers(typeof(Program).Assembly);
            // todo: add static methods (async and sync, no DI - useful for simple tasks eg redirect)
            // todo: use attribute for event name
            break;
        default:
            throw new InvalidOperationException("Unknown value for BSSE_SCALEOUT.");
    }

    // todo: scan for both types of event handler


    // todo: register in container / provider

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.UseBoomerSse();

app.Run();