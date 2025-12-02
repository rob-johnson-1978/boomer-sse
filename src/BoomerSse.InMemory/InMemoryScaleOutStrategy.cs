using BoomerSse.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoomerSse.InMemory;

public class InMemoryScaleOutStrategy : IDecideScaleOutStrategy
{
    public void Configure(IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IReceiveClientEvents>(ClientEventManager.Instance);
        builder.Services.AddSingleton<IReceiveServerEvents>(ClientEventManager.Instance);
    }
}