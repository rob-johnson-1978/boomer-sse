using BoomerSse.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    internal static IResult Sub(
        [FromServices] ClientStreamSubscriber clientStreamSubscriber,
        [FromServices] ServerStreamPublisher serverStreamPublisher,
        [FromQuery(Name = "session_id")] Guid sessionId,
        CancellationToken cancellationToken)
    {
        clientStreamSubscriber.StartSubscribing(sessionId, cancellationToken);

        return TypedResults.ServerSentEvents(
            serverStreamPublisher.StartPublishing(sessionId, cancellationToken)
        );
    }
}