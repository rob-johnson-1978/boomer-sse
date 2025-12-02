using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BoomerSse;

/// <summary>
/// BoomerSse script TagHelper
/// </summary>
/// <param name="fileVersionProvider"></param>
[HtmlTargetElement("bsse-script")]
public class BsseScriptTagHelper(IFileVersionProvider fileVersionProvider) : TagHelper
{
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    /// <summary>
    /// When true, disables browser caching
    /// </summary>
    public bool? NoCache { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "script";
        output.TagMode = TagMode.StartTagAndEndTag;

        var pathBase = ViewContext?.HttpContext.Request.PathBase ?? string.Empty;
        var src = $"{pathBase}/_content/BoomerSse/boomer-sse.js";

        var shouldDisableCache = NoCache ?? false;

        if (shouldDisableCache)
        {
            src = $"{src}?v={DateTime.UtcNow.Ticks}";
        }
        else if (ViewContext?.HttpContext.RequestServices != null)
        {
            src = fileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, src);
        }

        output.Attributes.SetAttribute("src", src);
    }
}