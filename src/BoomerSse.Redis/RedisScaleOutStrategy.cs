using BoomerSse.Abstractions;
using Microsoft.Extensions.Hosting;

namespace BoomerSse.Redis;

public class RedisScaleOutStrategy : IDecideScaleOutStrategy
{
    public void Configure(IHostApplicationBuilder builder)
    {
        
    }
}