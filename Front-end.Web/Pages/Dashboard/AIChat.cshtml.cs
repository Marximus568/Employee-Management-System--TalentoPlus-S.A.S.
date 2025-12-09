using Application.DTOs.AI;
using Application.Interfaces.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Front_end.Web.Pages.Dashboard;

[Authorize]
public class AIChatModel : PageModel
{
    private readonly IAiService _aiService;

    [BindProperty]
    public string UserMessage { get; set; } = string.Empty;

    public string AiResponse { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public AIChatModel(IAiService aiService)
    {
        _aiService = aiService;
    }

    public void OnGet()
    {
        // Initial page load
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(UserMessage))
        {
            return Page();
        }

        try
        {
            // Get AI response using AskGeminiAsync
            var request = new GeminiRequestDto
            {
                Prompt = UserMessage
            };
            
            var response = await _aiService.AskGeminiAsync(request);
            
            if (response.IsSuccess)
            {
                AiResponse = response.Content;
            }
            else
            {
                ErrorMessage = response.ErrorMessage ?? "Unknown error occurred";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}. Please make sure GEMINI_API_KEY is configured in your .env file.";
        }

        return Page();
    }
}
