using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Web.Pages.Dashboard.Excel;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
