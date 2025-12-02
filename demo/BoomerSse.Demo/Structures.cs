namespace BoomerSse.Demo;

internal sealed record HipHappened;

internal sealed record HipHappenedAgain(string Message);

internal sealed record MainButtonClicked(DateTime? Timestamp);

internal sealed record MainFormSubmitted(string? Prop1, string? Prop2, string? Prop3);