using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Web.Pages.Dashboard.Reports;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
