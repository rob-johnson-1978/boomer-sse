using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoomerSse.Demo.Pages.Home;

public class _HomeReadyModel(MyHomeReadyFunction function) : PageModel
{
    public void OnGet()
    {
        Time = function.GetTime();
    }

    public DateTime? Time { get; set; }
}