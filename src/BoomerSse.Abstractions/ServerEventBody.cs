namespace BoomerSse.Abstractions;

public sealed record ServerEventBody(RenderAction Action, string Details)
{
    public static ServerEventBody Default => new(RenderAction.None, "");
}