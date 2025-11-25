using System.Collections.Immutable;
using BoomerSse.Abstractions;

namespace BoomerSse;

public class ClientEventHandler
{
    static ClientEventHandler()
    {
    }

    private ClientEventHandler()
    {
    }

    public static ClientEventHandler Instance { get; } = new();

    public async Task<ImmutableArray<ServerEventBody>> Handle(
        ClientEventBody clientEventBody,
        CancellationToken cancellationToken)
    {
        // todo: probably need more params here - eg IServiceProvider? Or scoped service provider?
        throw new NotImplementedException();
    }
}