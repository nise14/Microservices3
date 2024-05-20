using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Mango.Web.Utility;
using Mango.Web.Service.IService;
using System.Text.Json;
using IdentityModel;

namespace Mango.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly IShoppingCartService _cartService;
    private static readonly JsonSerializerOptions _propertyCase = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HomeController(IProductService productService, IShoppingCartService cartService)
    {
        _productService = productService;
        _cartService = cartService;
    }
    public async Task<IActionResult> Index()
    {
        List<ProductDto?> list = [];

        ResponseDto? response = await _productService.GetAllProductsAsync();

        if (response != null && response.IsSuccess)
        {
            list = JsonSerializer.Deserialize<List<ProductDto>>(Convert.ToString(response.Result)!, _propertyCase)!;
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(list);
    }

    [Authorize]
    public async Task<IActionResult> ProductDetails(int productId)
    {
        ProductDto? model = new();

        ResponseDto? response = await _productService.GetProductByIdAsync(productId);

        if (response != null && response.IsSuccess)
        {
            model = JsonSerializer.Deserialize<ProductDto>(Convert.ToString(response.Result)!, _propertyCase);
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ActionName("ProductDetails")]
    public async Task<IActionResult> ProductDetails(ProductDto productDto)
    {
        CartDto cartDto = new()
        {
            CartHeader = new()
            {
                UserId = User.Claims.Where(u => u.Type == JwtClaimTypes.Subject)?.FirstOrDefault()?.Value
            }
        };

        CartDetailDto cartDetailDto = new()
        {
            Count = productDto.Count,
            ProductId = productDto.ProductId
        };

        List<CartDetailDto> cartDetailDtos = new() { cartDetailDto };
        cartDto.CartDetails = cartDetailDtos;

        ResponseDto? response = await _cartService.UpsertCartAsync(cartDto);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Item has been added to the Shopping Cart";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View();
    }

    [Authorize(Roles = SD.ROLEADMIN)]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
