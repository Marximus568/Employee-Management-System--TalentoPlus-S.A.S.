using Application.Interfaces;
using Application.Interfaces.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IAuthenticationService = Application.Interfaces.Identity.IAuthenticationService;

namespace Front_end.Web.Pages.Dashboard;


public class IndexModel : PageModel
{
    private readonly Application.Interfaces.Identity.IAuthenticationService _authService;

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
            UserRole = user.Role;
        }
    }
}
