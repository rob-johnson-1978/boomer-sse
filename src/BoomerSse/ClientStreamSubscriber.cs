namespace BoomerSse;

internal sealed class ClientStreamSubscriber
{
    internal void StartSubscribing(Guid sessionId, CancellationToken cancellationToken)
    {
        // todo: subscribe to client events for the given sessionId (from strategies)
        // each time an event is received call any event handlers to get the ServerEventBody contents
    }
}