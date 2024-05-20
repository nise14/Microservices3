namespace Mango.Services.AuthAPI.Models.Dto;

public class LoginResponseDto
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = null!;
}