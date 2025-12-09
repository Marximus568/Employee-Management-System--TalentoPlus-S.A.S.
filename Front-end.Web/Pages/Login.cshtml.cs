using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Web.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthenticationService _authService;

    [BindProperty]
    public LoginDto Input { get; set; } = new LoginDto();

    public string? ErrorMessage { get; set; }

    public LoginModel(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public void OnGet()
    {
        // Display login page
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.LoginAsync(Input);

        if (result.Success)
        {
            // Redirect to dashboard on successful login
            return RedirectToPage("/Dashboard/Index");
        }

        // Show error message
        ErrorMessage = result.ErrorMessage;
        return Page();
    }
}
