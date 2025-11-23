using System.Text.Json.Serialization;

namespace BoomerSse.Abstractions;

public sealed record ClientEventBody(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("data")] string? Data
);