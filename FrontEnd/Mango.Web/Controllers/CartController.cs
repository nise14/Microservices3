using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers;

public class CartController : Controller
{
    private readonly IShoppingCartService _cartService;

    private static readonly JsonSerializerOptions _propertyCase = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CartController(IShoppingCartService cartService)
    {
        _cartService = cartService;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        return View(await LoadCartDtoBasedOnLoggedInUser());
    }

    public async Task<IActionResult> Remove(int cartDetailsId)
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
        ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailsId);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
    {
        ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> EmailCart(CartDto cartDto)
    {
        CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
        cart.CartHeader!.Email = User.Claims.Where(u=>u.Type==JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
        ResponseDto? response = await _cartService.EmailCart(cart);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Email will be processed and sent shortly.";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCoupon(CartDto cartDto){
        cartDto.CartHeader!.CouponCode = "";
        ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
        ResponseDto? response = await _cartService.GetCartByUserIdAsync(userId!);

        if (response != null && response.IsSuccess)
        {
            CartDto cartDto = JsonSerializer.Deserialize<CartDto>(Convert.ToString(response.Result)!, _propertyCase)!;
            return cartDto;
        }

        return new CartDto();
    }
}