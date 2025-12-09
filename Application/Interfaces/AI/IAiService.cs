using Application.DTOs.AI;

namespace Application.Interfaces.AI;

/// <summary>
/// Service contract for AI operations.
/// Defines the interface for interacting with AI models (Gemini, OpenAI, etc.)
/// following the Dependency Inversion Principle.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Sends a prompt to Gemini AI and retrieves the generated response.
    /// </summary>
    /// <param name="request">The request containing the prompt and configuration.</param>
    /// <returns>A response object containing the generated text and metadata.</returns>
    Task<GeminiResponseDto> AskGeminiAsync(GeminiRequestDto request);

    /// <summary>
    /// Optional: Validates if the AI service is properly configured and accessible.
    /// </summary>
    /// <returns>True if the service is healthy, false otherwise.</returns>
    Task<bool> IsHealthyAsync();
}
