using Microsoft.Extensions.Hosting;

namespace BoomerSse.Abstractions;

public interface IDecideScaleOutStrategy
{
    void Configure(IHostApplicationBuilder builder);
}