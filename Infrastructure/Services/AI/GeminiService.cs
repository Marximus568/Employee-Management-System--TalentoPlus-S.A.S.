using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces.AI;

namespace Infrastructure.AI;

/// <summary>
/// Implementation of IAIService using Google Gemini REST API.
/// Makes direct HTTP calls to Gemini without requiring the official SDK.
/// This approach is more flexible and doesn't depend on NuGet packages.
/// </summary>
public class GeminiService : IAiService
{
    private readonly string _apiKey;
    private readonly ILogger<GeminiService>? _logger;
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private const string GEMINI_API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models";

    /// <summary>
    /// Constructor that initializes the Gemini service with API credentials.
    /// </summary>
    /// <param name="apiKey">The Google Gemini API key for authentication.</param>
    /// <param name="logger">Optional logger for debugging and monitoring.</param>
    /// <param name="httpClient">Optional HTTP client. If null, creates a new one.</param>
    /// <param name="modelName">Model to use. Default: gemini-pro</param>
    public GeminiService(
        string apiKey, 
        ILogger<GeminiService>? logger = null,
        HttpClient? httpClient = null,
        string modelName = "gemini-pro")
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));

        _apiKey = apiKey;
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient();
        _modelName = modelName;

        _logger?.LogInformation("GeminiService initialized with model: {Model}", _modelName);
    }

    /// <summary>
    /// Sends a prompt to Gemini AI and returns the generated response.
    /// Makes a direct HTTP POST request to the Gemini REST API.
    /// </summary>
    public async Task<GeminiResponseDto> AskGeminiAsync(GeminiRequestDto request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                _logger?.LogWarning("Empty prompt received.");
                return new GeminiResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Prompt cannot be empty."
                };
            }

            _logger?.LogInformation("Sending request to Gemini AI. Prompt length: {Length}", request.Prompt.Length);

            // Build API endpoint
            string endpoint = $"{GEMINI_API_BASE_URL}/{_modelName}:generateContent?key={_apiKey}";

            // Build request payload
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = request.Prompt }
                        }
                    }
                },
                generationConfig = BuildGenerationConfig(request)
            };

            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Send HTTP request
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("Gemini API error. Status: {Status}, Body: {Body}", 
                    response.StatusCode, responseBody);

                return new GeminiResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"API Error ({response.StatusCode}): {ExtractErrorMessage(responseBody)}"
                };
            }

            // Parse response
            var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseBody);
            string generatedText = ExtractTextFromResponse(geminiResponse);

            _logger?.LogInformation("Received response from Gemini AI. Response length: {Length}", 
                generatedText.Length);

            return new GeminiResponseDto
            {
                Content = generatedText,
                IsSuccess = true,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while calling Gemini API.");
            return new GeminiResponseDto
            {
                IsSuccess = false,
                ErrorMessage = $"Network error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while calling Gemini AI.");
            return new GeminiResponseDto
            {
                IsSuccess = false,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Checks if the Gemini service is properly configured and accessible.
    /// </summary>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var testRequest = new GeminiRequestDto
            {
                Prompt = "Hello"
            };

            var response = await AskGeminiAsync(testRequest);
            return response.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Health check failed for Gemini service.");
            return false;
        }
    }

    /// <summary>
    /// Builds the generation config object, only including non-null values.
    /// This prevents sending unnecessary default values to the API.
    /// </summary>
    private object? BuildGenerationConfig(GeminiRequestDto request)
    {
        // Only create config if at least one parameter is specified
        if (!request.Temperature.HasValue && !request.MaxTokens.HasValue)
            return null;

        var config = new Dictionary<string, object>();

        if (request.Temperature.HasValue)
            config["temperature"] = request.Temperature.Value;

        if (request.MaxTokens.HasValue)
            config["maxOutputTokens"] = request.MaxTokens.Value;

        return config.Count > 0 ? config : null;
    }

    /// <summary>
    /// Extracts the generated text from Gemini's response structure.
    /// </summary>
    private string ExtractTextFromResponse(GeminiApiResponse? response)
    {
        if (response?.Candidates == null || response.Candidates.Length == 0)
            return string.Empty;

        var firstCandidate = response.Candidates[0];
        if (firstCandidate?.Content?.Parts == null || firstCandidate.Content.Parts.Length == 0)
            return string.Empty;

        return firstCandidate.Content.Parts[0]?.Text ?? string.Empty;
    }

    /// <summary>
    /// Extracts error message from API error response.
    /// </summary>
    private string ExtractErrorMessage(string responseBody)
    {
        try
        {
            var errorResponse = JsonSerializer.Deserialize<GeminiErrorResponse>(responseBody);
            return errorResponse?.Error?.Message ?? "Unknown error";
        }
        catch
        {
            return responseBody.Length > 200 ? responseBody[..200] + "..." : responseBody;
        }
    }

    #region Response Models (for JSON deserialization)

    private class GeminiApiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public Part[]? Parts { get; set; }
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class GeminiErrorResponse
    {
        [JsonPropertyName("error")]
        public ErrorDetails? Error { get; set; }
    }

    private class ErrorDetails
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }
    }

    #endregion
}