using System.Collections.Immutable;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BoomerSse;

/// <summary>
/// Base TagHelper for HTML elements that publish client events
/// </summary>
[HtmlTargetElement(Attributes = "*")]
public class BsseHtmlElementTagHelper : TagHelper
{
    private static readonly ImmutableArray<string> AllowedJavaScriptEvents = [
        "click", "load", "submit" // todo: add more events as needed
    ];

    /// <summary>
    /// The JavaScript event that will trigger the publishing of the event
    /// </summary>
    [HtmlAttributeName("on")]
    public string? On { get; set; }

    /// <summary>
    /// Gets or sets the name of the event to be published when the associated action is triggered
    /// </summary>
    [HtmlAttributeName("publish")]
    public string? EventName { get; set; }

    /// <summary>
    /// Gets or sets the message (string) to be sent with the event
    /// </summary>
    [HtmlAttributeName("with-message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the data to be sent with the event as JSON
    /// </summary>
    [HtmlAttributeName("with-data")]
    public object? Data { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (
            string.IsNullOrWhiteSpace(On) ||
            !AllowedJavaScriptEvents.Contains(On) ||
            string.IsNullOrWhiteSpace(EventName)
        )
        {
            return;
        }

        output.Attributes.SetAttribute($"on{On}", "publishClientEvent(event)");
        output.Attributes.SetAttribute("data-bsse-event", EventName);

        if (!string.IsNullOrWhiteSpace(Message) && Data == null)
        {
            output.Attributes.SetAttribute("data-bsse-message", WebUtility.HtmlEncode(Message));
        }
        
        if (Data != null)
        {
            output.Attributes.SetAttribute("data-bsse-data", WebUtility.HtmlEncode(JsonSerializer.Serialize(Data)));
        }

        base.Process(context, output);
    }
}