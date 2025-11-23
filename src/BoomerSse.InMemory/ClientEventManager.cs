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
    
    public async Task Receive(Guid sessionId, ClientEventBody clientEventBody, CancellationToken cancellationToken)
    {
        // todo: put message on inmem queue?
        throw new NotImplementedException();
    }
}