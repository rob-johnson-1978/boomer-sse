using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using BoomerSse.Abstractions;

namespace BoomerSse;

internal sealed class ServerStreamPublisher
{
    internal async IAsyncEnumerable<SseItem<ServerEventBody>> StartPublishing(
        Guid sessionId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var yieldChannel = Channel.CreateUnbounded<SseItem<ServerEventBody>>();

        // todo: hook into strategies to start producing events for the sessionId (pushing to yieldChannel.Writer)

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!await yieldChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    continue;
                }

                while (yieldChannel.Reader.TryRead(out var sseItem))
                {
                    yield return sseItem;
                }
            }
        }
        finally
        {
            yieldChannel.Writer.Complete();

            // todo: dispose of any other resources associated with the sessionId (from strategies)
        }
    }
}