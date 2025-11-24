using BoomerSse.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.ServerSentEvents;
using System.Threading.Channels;

namespace BoomerSse;

internal class RequestHandlers
{
    internal static async Task<IResult> Pub(
        [FromServices] IReceiveClientEvents clientEventReceiver,
        [FromQuery(Name = "session_id")] Guid sessionId,
        [FromBody] ClientEventBody clientEventBody,
        CancellationToken cancellationToken)
    {
        await clientEventReceiver.Receive(sessionId, clientEventBody, cancellationToken);

        return Results.Accepted();
    }

    internal static async Task<IResult> Sub(
        [FromServices] IReceiveClientEvents clientEventReceiver,
        [FromQuery(Name = "session_id")] Guid sessionId,
        CancellationToken cancellationToken)
    {
        // todo:
        // 1. setup a client event handler
        // 2. subscribe to the client event and forward to the handler
        // 3. onHandled, yield the server events from the yielder

        var yieldChannel = Channel.CreateUnbounded<SseItem<ServerEventBody>>();

        return TypedResults.ServerSentEvents(
            Yield()
        );

        // -------------------------------- //

        async IAsyncEnumerable<SseItem<ServerEventBody>> Yield()
        {
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
}