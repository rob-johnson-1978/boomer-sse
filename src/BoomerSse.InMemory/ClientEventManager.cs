using BoomerSse.Abstractions;

namespace BoomerSse.InMemory;

public class ClientEventManager : IReceiveClientEvents
{
    static ClientEventManager()
    {
    }

    private ClientEventManager()
    {
    }

    public static ClientEventManager Instance { get; } = new();
    
    public async Task Receive(Guid sessionId, ClientEventBody clientEventBody)
    {
        // todo: some kind of streaming solution
        // it can't just be a ConcurrentDictionary<Guid, List<ClientEventBody>> because of potential memory overflow
        // needs to be a stream/queue/channel etc
    }
}