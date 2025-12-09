namespace Application.DTOs.AI;

public class GeminiRequestDto
{
    /// <summary>
    /// The prompt/question that will be sent to Gemini AI.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Temperature for response generation (0.0 to 2.0).
    /// Higher values make output more random, lower values more deterministic.
    /// If null, Gemini will use its default value.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Optional: Maximum number of tokens to generate.
    /// If null, Gemini will use its default value.
    /// </summary>
    public int? MaxTokens { get; set; }
}