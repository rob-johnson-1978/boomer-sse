using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BoomerSse;

internal class RequestHandlers
{
    internal static async Task<IResult> ReceiveClientEvent(
        [FromServices] ILogger<RequestHandlers> logger,
        [FromQuery(Name = "session_id")] Guid sessionId,
        [FromBody] ClientEventBody clientEventBody)
    {
        logger.LogInformation("{s}", sessionId);
        logger.LogInformation("{e}", clientEventBody);

        await Task.CompletedTask;
        
        return Results.Accepted();
    }
}