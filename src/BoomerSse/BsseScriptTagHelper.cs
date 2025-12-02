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

    /// <summary>
    /// When set, will be used to get the bearer token. Eg "sessionStorage.getItem('token')"
    /// </summary>
    public string GetBearerTokenJs { get; set; } = "bsse.defaultGetToken()";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var pathBase = ViewContext?.HttpContext.Request.PathBase ?? string.Empty;

        var globalVariablesScript = new TagBuilder("script");
        globalVariablesScript.Attributes.Add("type", "text/javascript");
        globalVariablesScript.InnerHtml.AppendHtml(
$@"
    BSSE_SESSION_ID = '{Guid.NewGuid()}';
    BSSE_PATH_BASE = '{pathBase}';
    BSSE_GET_TOKEN = () => {GetBearerTokenJs};
");

        // Create main script tag
        var mainScript = new TagBuilder("script");

        var src = $"{pathBase}/_content/BoomerSse/boomer-sse.js";

        var shouldDisableCache = NoCache ?? false;

        if (shouldDisableCache)
        {
            // Force reload with timestamp to prevent caching
            src = $"{src}?v={DateTime.UtcNow.Ticks}";
        }
        else if (ViewContext?.HttpContext.RequestServices != null)
        {
            // Add version hash for cache busting (allows caching until file changes)
            src = fileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, src);
        }

        mainScript.Attributes.Add("src", src);

        // Replace the tag with both scripts
        output.TagName = null; // Remove the original tag
        output.PostContent.AppendHtml(globalVariablesScript);
        output.PostContent.AppendHtml(mainScript);
    }
}