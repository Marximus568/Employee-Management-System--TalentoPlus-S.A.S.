using Application.Interfaces;
using Application.Interfaces.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Web.Pages;

public class LogoutModel : PageModel
{
    private readonly IAuthenticationService _authService;

    public LogoutModel(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _authService.LogoutAsync();
        return RedirectToPage("/Index");
    }
}
