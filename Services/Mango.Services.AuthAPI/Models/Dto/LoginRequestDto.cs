namespace Mango.Services.AuthAPI.Models.Dto;

public class LoginRequestDto
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}