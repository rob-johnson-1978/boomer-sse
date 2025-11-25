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
        [FromServices] ClientEventHandler clientEventHandler,
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
                clientEventHandler,
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
        ClientEventHandler clientEventHandler,
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
                    var serverEventBodies = await clientEventHandler.Handle(clientEventBody, cancellationToken);
                    
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