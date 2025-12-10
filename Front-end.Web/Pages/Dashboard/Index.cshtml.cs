using Application.Interfaces;
using Application.Interfaces.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IAuthenticationService = Application.Interfaces.Identity.IAuthenticationService;

namespace Front_end.Web.Pages.Dashboard;

[Authorize]
public class IndexModel : PageModel
{
    private readonly Application.Interfaces.Identity.IAuthenticationService _authService;
    private readonly Microsoft.AspNetCore.Identity.UserManager<Infrastructure.Models.ApplicationUser> _userManager;

    public string UserName { get; set; } = "User";
    public string UserRole { get; set; } = "User";
    
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }

    public IndexModel(
        Application.Interfaces.Identity.IAuthenticationService authService,
        Microsoft.AspNetCore.Identity.UserManager<Infrastructure.Models.ApplicationUser> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user != null)
        {
            UserName = user.FullName;
            UserRole = user.Role;
        }

        // Fetch Stats
        TotalUsers = _userManager.Users.Count();
        ActiveUsers = _userManager.Users.Count(u => u.IsActive);
        InactiveUsers = _userManager.Users.Count(u => !u.IsActive);
    }
}
