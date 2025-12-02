namespace BoomerSse.Demo;

internal record MainLoaded(DateTime Timestamp);

internal sealed record MainButtonClickedAgain(DateTime Timestamp);

internal sealed record MainFormSubmitted(string? Prop1, string? Prop2, string? Prop3);