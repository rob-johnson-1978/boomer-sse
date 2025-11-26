using BoomerSse.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
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
        await clientEventReceiver.ReceiveClientEvent(sessionId, clientEventBody, cancellationToken);

        return Results.Accepted();
    }

    internal static async Task<IResult> Sub(
        [FromServices] IReceiveClientEvents clientEventReceiver,
        [FromServices] IReceiveServerEvents serverEventReceiver,
        [FromServices] BoomerSseOptions options,
        [FromQuery(Name = "session_id")] Guid sessionId,
        CancellationToken cancellationToken)
    {
        var clientEventHandlingChannel = Channel.CreateUnbounded<ClientEventBody>();
        var serverEventYieldingChannel = Channel.CreateUnbounded<SseItem<ServerEventBody>>();

        await clientEventReceiver.SubscribeToClientEvents(
            sessionId,
            async clientEventBody =>
            {
                await clientEventHandlingChannel
                    .Writer
                    .WriteAsync(clientEventBody, cancellationToken);
            },
            cancellationToken
        );

        _ = Task.Run(async () => 
            await HandleClientEvents(
                sessionId, 
                clientEventHandlingChannel,
                options,
                serverEventReceiver,
                cancellationToken
            ), cancellationToken
        );

        await serverEventReceiver.SubscribeToServerEvents(
            sessionId,
            async serverEventBody =>
            {
                await serverEventYieldingChannel
                    .Writer
                    .WriteAsync(
                        new SseItem<ServerEventBody>(serverEventBody),
                        cancellationToken
                    );
            },
            cancellationToken
        );

        return TypedResults.ServerSentEvents(
            Yield(serverEventYieldingChannel, cancellationToken)
        );
    }

    private static async Task HandleClientEvents(Guid sessionId,
        Channel<ClientEventBody> clientEventHandlingChannel,
        BoomerSseOptions options,
        IReceiveServerEvents serverEventReceiver,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!await clientEventHandlingChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    continue;
                }

                while (clientEventHandlingChannel.Reader.TryRead(out var clientEventBody))
                {
                    // todo:
                    // 1. Find the handler/s in the options, for this clientEventBody.Event (that's the event name)
                    // 2. Execute each handler, in parallel (await Task.WhenAll)
                    // 3. Aggregate the results into serverEventBodies
                    
                    await serverEventReceiver.ReceiveServerEvents(sessionId, serverEventBodies, cancellationToken);
                }
            }
            finally
            {
                clientEventHandlingChannel.Writer.Complete();
            }
        }
    }

    private static async IAsyncEnumerable<SseItem<ServerEventBody>> Yield(
        Channel<SseItem<ServerEventBody>> serverEventYieldingChannel,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!await serverEventYieldingChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    continue;
                }

                while (serverEventYieldingChannel.Reader.TryRead(out var sseItem))
                {
                    yield return sseItem;
                }
            }
        }
        finally
        {
            serverEventYieldingChannel.Writer.Complete();
        }
    }
}