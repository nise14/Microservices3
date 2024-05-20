using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mango.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ITokenProvider _tokenProvider;

    private static readonly JsonSerializerOptions optionsJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthController(IAuthService authService, ITokenProvider tokenProvider)
    {
        _authService = authService;
        _tokenProvider = tokenProvider;
    }

    [HttpGet]
    public IActionResult Login()
    {
        LoginRequestDto loginRequestDto = new();
        return View(loginRequestDto);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto model)
    {
        ResponseDto? responseDto = await _authService.LoginAsync(model);

        if (responseDto is not null && responseDto.IsSuccess)
        {
            LoginResponseDto loginResponseDto =
                JsonSerializer.Deserialize<LoginResponseDto>(Convert.ToString(responseDto.Result)!, optionsJson)!;

            await SignInUser(loginResponseDto);
            _tokenProvider.SetToken(loginResponseDto.Token);
            return RedirectToAction("Index", "Home");
        }
        else{
            TempData["error"]= responseDto?.Message;
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        var roleList = new List<SelectListItem>{
            new SelectListItem{
                Text = SD.ROLEADMIN,
                Value = SD.ROLEADMIN,
            },
            new SelectListItem{
                Text = SD.ROLECUSTOMER,
                Value = SD.ROLECUSTOMER,
            }
        };

        ViewBag.RoleList = roleList;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegistrationRequestDto model)
    {
        ResponseDto? result = await _authService.RegisterAsync(model);
        ResponseDto? assigneRole;

        if (result is not null && result.IsSuccess)
        {
            if (string.IsNullOrWhiteSpace(model.Role))
            {
                model.Role = SD.ROLECUSTOMER;
            }

            assigneRole = await _authService.AssignRoleAsync(model);

            if (assigneRole is not null && assigneRole.IsSuccess)
            {
                TempData["success"] = "Registration Successful";
                return RedirectToAction(nameof(Login));
            }
        }
        else{
            TempData["error"]= result?.Message;
        }

        var roleList = new List<SelectListItem>{
            new SelectListItem{
                Text = SD.ROLEADMIN,
                Value = SD.ROLEADMIN,
            },
            new SelectListItem{
                Text = SD.ROLECUSTOMER,
                Value = SD.ROLECUSTOMER,
            }
        };

        ViewBag.RoleList = roleList;

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        _tokenProvider.ClearToken();
        return RedirectToAction("Index","Home");
    }

    private async Task SignInUser(LoginResponseDto model)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(model.Token);
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email, jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email)!.Value));
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)!.Value));
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name, jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name)!.Value));
        identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email)!.Value));

        identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role")!.Value));

        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}