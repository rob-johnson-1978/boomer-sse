using BoomerSse.Abstractions;

namespace BoomerSse.InMemory;

public class ClientEventManager : IReceiveClientEvents, IReceiveServerEvents
{
    static ClientEventManager()
    {
    }

    private ClientEventManager()
    {
    }

    public static ClientEventManager Instance { get; } = new();
}