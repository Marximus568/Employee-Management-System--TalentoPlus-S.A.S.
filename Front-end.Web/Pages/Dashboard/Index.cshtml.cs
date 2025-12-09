using Application.Interfaces;
using Application.Interfaces.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Web.Pages.Dashboard;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IAuthenticationService _authService;

    public string UserName { get; set; } = "User";
    public string UserRole { get; set; } = "User";

    public IndexModel(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task OnGetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user != null)
        {
            UserName = user.FullName;
            // You can get role from UserManager if needed
        }
    }
}
