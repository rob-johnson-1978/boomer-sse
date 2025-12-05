using BoomerSse.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace BoomerSse;

public interface IProvideRazorFunctionality
{
    Task<ServerEventBody> RenderPartial(
        string viewPath,
        RenderAction renderAction
    );
}

public class RazorFunctionality : IProvideRazorFunctionality
{
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RazorFunctionality(
        IRazorViewEngine razorViewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServerEventBody> RenderPartial(string viewPath,
        RenderAction renderAction)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? new DefaultHttpContext { RequestServices = _serviceProvider };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        await using var writer = new StringWriter();

        var viewResult = _razorViewEngine.FindView(actionContext, viewPath, false);

        if (!viewResult.Success)
        {
            viewResult = _razorViewEngine.GetView(
                executingFilePath: null,
                viewPath, 
                false
            );
        }

        if (!viewResult.Success)
        {
            throw new InvalidOperationException(
                $"View '{viewPath}' not found. Searched locations: " +
                $"{string.Join(", ", viewResult.SearchedLocations)}"
            );
        }

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);

        var renderedHtml = writer.ToString();

        return new ServerEventBody(renderAction.ToString(), renderedHtml);
    }
}