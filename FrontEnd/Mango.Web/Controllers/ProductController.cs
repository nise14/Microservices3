using System.Text.Json;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private static readonly JsonSerializerOptions _propertyCase = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        List<ProductDto?> list = new();

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

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductDto model)
    {
        if (ModelState.IsValid)
        {
            ResponseDto? response = await _productService.CreateProductsAsync(model);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Delete(int productId)
    {
        ResponseDto? response = await _productService.GetProductByIdAsync(productId);

        if (response != null && response.IsSuccess)
        {
            ProductDto? model = JsonSerializer.Deserialize<ProductDto>(Convert.ToString(response.Result)!, _propertyCase)!;
            return View(model);
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(ProductDto productDto)
    {
        ResponseDto? response = await _productService.DeleteProductsAsync(productDto.ProductId);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(productDto);
    }

    public async Task<IActionResult> Edit(int productId)
    {
        ResponseDto? response = await _productService.GetProductByIdAsync(productId);

        if (response != null && response.IsSuccess)
        {
            ProductDto? model = JsonSerializer.Deserialize<ProductDto>(Convert.ToString(response.Result)!, _propertyCase)!;
            return View(model);
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProductDto productDto)
    {
        ResponseDto? response = await _productService.UpdateProductsAsync(productDto);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(productDto);
    }
}