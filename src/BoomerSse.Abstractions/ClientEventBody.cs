using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoomerSse.Abstractions;

public sealed record ClientEventBody(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("data")] JsonDocument? Data
);