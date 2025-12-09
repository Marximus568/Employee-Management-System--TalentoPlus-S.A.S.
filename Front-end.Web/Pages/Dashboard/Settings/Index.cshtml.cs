using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Web.Pages.Dashboard.Settings;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
