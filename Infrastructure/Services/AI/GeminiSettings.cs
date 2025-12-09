namespace Infrastructure.Services.AI;

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-pro";
    public string Endpoint { get; set; } =
        "https://generativelanguage.googleapis.com/v1/models/{0}:generateContent?key={1}";
}