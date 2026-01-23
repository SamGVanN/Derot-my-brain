using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.Core.DTOs;

public class LoginDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Language { get; set; }
    public string? PreferredTheme { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}
