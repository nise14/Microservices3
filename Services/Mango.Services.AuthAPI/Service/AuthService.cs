using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Service;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<bool> AssignRole(string email, string roleName)
    {
        var user = _context.ApplicationUsers.FirstOrDefault(u => u.Email! == email);
        if (user != null)
        {
            if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
            }

            await _userManager.AddToRoleAsync(user, roleName);
            return true;
        }

        return false;
    }

    public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
    {
        var user = _context.ApplicationUsers.FirstOrDefault(u => u.UserName == loginRequestDto.UserName);

        bool isValid = await _userManager.CheckPasswordAsync(user!, loginRequestDto.Password);

        if (user is null || !isValid)
        {
            return new LoginResponseDto()
            {
                User = null!,
                Token = string.Empty
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        UserDto userDto = new()
        {
            Email = user.Email!,
            Id = user.Id,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber!
        };

        LoginResponseDto loginResponseDto = new()
        {
            User = userDto,
            Token = token
        };

        return loginResponseDto;
    }

    public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
    {
        ApplicationUser user = new()
        {
            UserName = registrationRequestDto.Email,
            Email = registrationRequestDto.Email,
            NormalizedEmail = registrationRequestDto.Email.ToUpper(),
            Name = registrationRequestDto.Name,
            PhoneNumber = registrationRequestDto.PhoneNumber
        };

        try
        {
            var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
            if (result.Succeeded)
            {
                var userToReturn = _context.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);

                UserDto userDto = new()
                {
                    Email = userToReturn.Email!,
                    Id = userToReturn.Id,
                    Name = userToReturn.Name,
                    PhoneNumber = userToReturn.PhoneNumber!
                };

                return string.Empty;
            }
            else
            {
                return result.Errors.FirstOrDefault()?.Description!;
            }
        }
        catch (Exception)
        {
        }

        return "Error encountered";
    }
}