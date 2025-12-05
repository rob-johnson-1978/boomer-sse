using BoomerSse.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace BoomerSse;

internal class RequestHandlers
{
    internal static async Task<IResult> Pub(
        [FromServices] ILogger<RequestHandlers> logger,
        [FromServices] IReceiveClientEvents clientEventReceiver,
        [FromQuery(Name = "session_id")] Guid sessionId,
        [FromBody] ClientEventBody clientEventBody,
        CancellationToken cancellationToken)
    {
        try
        {
            await clientEventReceiver.ReceiveClientEvent(sessionId, clientEventBody, cancellationToken);

            return Results.Accepted();
        }
        catch (Exception ex)
        {
            var problemDetails = new ProblemDetails
            {
                Detail = $"{ex.Message}{(ex.InnerException == null ? "" : " " + ex.InnerException.Message)}",
                Status = 500,
                Title = "An error occurred while processing the client event"
            };

            logger.LogError(ex, "An error occurred when the client POSTed an event to the server");

            return Results.Problem(problemDetails);
        }
    }

    internal static async Task<IResult> Sub(
        [FromServices] ILogger<RequestHandlers> logger,
        [FromServices] IServiceProvider scopedServiceProvider,
        [FromServices] IReceiveClientEvents clientEventReceiver,
        [FromServices] IReceiveServerEvents serverEventReceiver,
        [FromQuery(Name = "session_id")] Guid sessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var clientEventHandlingChannel = Channel.CreateUnbounded<ClientEventBody>();
            var serverEventYieldingChannel = Channel.CreateUnbounded<SseItem<ServerEventBody>>();

            await clientEventReceiver.SubscribeToClientEvents(
                sessionId,
                async clientEventBody =>
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    try
                    {
                        await clientEventHandlingChannel.Writer.WaitToWriteAsync(cancellationToken);
                        await clientEventHandlingChannel.Writer.WriteAsync(clientEventBody, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancelling
                    }
                },
                cancellationToken
            );

            _ = Task.Run(async () =>
                    await HandleClientEvents(
                        logger,
                        sessionId,
                        clientEventHandlingChannel,
                        serverEventReceiver,
                        scopedServiceProvider,
                        cancellationToken
                    ),
                    cancellationToken
                );

            await serverEventReceiver.SubscribeToServerEvents(
                sessionId,
                async serverEventBody =>
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    try
                    {
                        await serverEventYieldingChannel.Writer.WaitToWriteAsync(cancellationToken);
                        await serverEventYieldingChannel.Writer.WriteAsync(
                            new SseItem<ServerEventBody>(serverEventBody),
                            cancellationToken
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancelling
                    }
                },
                cancellationToken
            );

            // Clean up when connection ends
            cancellationToken.Register(async () =>
            {
                await clientEventReceiver.StopSubscribingToClientEvents(sessionId);
                await serverEventReceiver.StopSubscribingToServerEvents(sessionId);

                clientEventHandlingChannel.Writer.TryComplete();
                serverEventYieldingChannel.Writer.TryComplete();
            });

            return TypedResults.ServerSentEvents(
                Yield(serverEventYieldingChannel, cancellationToken)
            );
        }
        catch (Exception ex)
        {
            var problemDetails = new ProblemDetails
            {
                Detail = $"{ex.Message}{(ex.InnerException == null ? "" : " " + ex.InnerException.Message)}",
                Status = 500,
                Title = "An error occurred while processing the client event"
            };

            logger.LogError(ex, "An error occurred when the client was connecting to the server for SSE");

            return Results.Problem(problemDetails);
        }
    }

    private static async Task HandleClientEvents(
        ILogger<RequestHandlers> logger,
        Guid sessionId,
        Channel<ClientEventBody> clientEventHandlingChannel,
        IReceiveServerEvents serverEventReceiver,
        IServiceProvider scopedServiceProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!await clientEventHandlingChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    break; // Channel closed
                }

                while (clientEventHandlingChannel.Reader.TryRead(out var clientEventBody))
                {
                    var serverEventBodies = await Handling.BuildClientEventHandlingTask(
                        clientEventBody,
                        scopedServiceProvider,
                        cancellationToken
                    );

                    await serverEventReceiver.ReceiveServerEvents(sessionId, serverEventBodies, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException or OperationCanceledException)
            {
                return;
            }

            logger.LogError(ex, "An exception occured during client event processing");
        }
        finally
        {
            clientEventHandlingChannel.Writer.TryComplete();
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
                try
                {
                    if (!await serverEventYieldingChannel.Reader.WaitToReadAsync(cancellationToken))
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                while (serverEventYieldingChannel.Reader.TryRead(out var sseItem))
                {
                    yield return sseItem;
                }
            }
        }
        finally
        {
            serverEventYieldingChannel.Writer.TryComplete();
        }
    }
}