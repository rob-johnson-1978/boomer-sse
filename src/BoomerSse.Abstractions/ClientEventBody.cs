using System.Text.Json.Serialization;

namespace BoomerSse.Abstractions;

public record ClientEventBody(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("data")] string? Data
);