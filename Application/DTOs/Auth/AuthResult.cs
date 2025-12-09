namespace Application.DTOs.Auth;

/// <summary>
/// DTO for authentication operation results
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public UserDto? User { get; set; }

    public static AuthResult SuccessResult(UserDto user)
    {
        return new AuthResult
        {
            Success = true,
            User = user
        };
    }

    public static AuthResult FailureResult(string errorMessage)
    {
        return new AuthResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
