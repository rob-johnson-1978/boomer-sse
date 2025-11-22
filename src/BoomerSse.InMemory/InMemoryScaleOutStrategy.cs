using BoomerSse.Abstractions;
using Microsoft.Extensions.Hosting;

namespace BoomerSse.InMemory;

public class InMemoryScaleOutStrategy : IDecideScaleOutStrategy
{
    public void Configure(IHostApplicationBuilder builder)
    {
        
    }
}