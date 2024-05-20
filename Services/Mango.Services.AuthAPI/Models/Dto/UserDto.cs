namespace Mango.Services.AuthAPI.Models.Dto;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}