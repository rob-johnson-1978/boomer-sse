namespace BoomerSse.Abstractions;

public sealed record ServerEventBody(string Action, string Details)
{
    public static ServerEventBody Default => new(nameof(RenderAction.None), "");
}