using BoomerSse.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BoomerSse;

internal class RequestHandlers
{
    internal static async Task<IResult> ReceiveClientEvent(
        [FromServices] ILogger<RequestHandlers> logger,
        [FromServices] IReceiveClientEvents clientEventReceiver,
        [FromQuery(Name = "session_id")] Guid sessionId,
        [FromBody] ClientEventBody clientEventBody)
    {
        await clientEventReceiver.Receive(sessionId, clientEventBody);
        
        return Results.Accepted();
    }
}