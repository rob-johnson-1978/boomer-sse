using BoomerSse;
using BoomerSse.InMemory;
using BoomerSse.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.UseBoomerSse(options =>
{
    switch (Environment.GetEnvironmentVariable("BSSE_SCALEOUT"))
    {
        case "InMemory":
            options.UseScaleOutStrategy(new InMemoryScaleOutStrategy());
            break;
        case "Redis":
            options.UseScaleOutStrategy(new RedisScaleOutStrategy());
            break;
        default:
            throw new InvalidOperationException("Unknown value for BSSE_SCALEOUT.");
    }
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