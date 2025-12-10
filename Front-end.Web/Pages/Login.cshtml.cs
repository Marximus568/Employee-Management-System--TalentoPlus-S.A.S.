using Application.DTOs.Auth;
using Application.Interfaces.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using IAuthenticationService = Application.Interfaces.Identity.IAuthenticationService;

namespace Front_end.Web.Pages
{
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

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Attempt login
            var result = await _authService.LoginAsync(Input);

            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return Page();
            }

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId),
                new Claim(ClaimTypes.Name, result.FullName ?? Input.Email),
                new Claim(ClaimTypes.Role, result.Role) // Add role claim
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Keep user logged in
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // Sign in the user with cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // Optional: redirect based on role
            if (result.Role == "Admin")
            {
                return RedirectToPage("/Dashboard/Index"); // Admin dashboard
            }
            else
            {
                return RedirectToPage("/Index"); // Regular user home
            }
        }
    }
}
