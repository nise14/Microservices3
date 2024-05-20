using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models;

public class RegistrationRequestDto
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    public string? Role { get; set; }
}