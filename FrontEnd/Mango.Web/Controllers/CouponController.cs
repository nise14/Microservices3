using System.Text.Json;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers;

public class CouponController : Controller
{
    private readonly ICouponService _couponService;
    private static readonly JsonSerializerOptions _propertyCase = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    public async Task<IActionResult> Index()
    {
        List<CouponDto?> list = new();

        ResponseDto? response = await _couponService.GetAllCouponsAsync();

        if (response != null && response.IsSuccess)
        {
            list = JsonSerializer.Deserialize<List<CouponDto>>(Convert.ToString(response.Result)!, _propertyCase)!;
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
    public async Task<IActionResult> Create(CouponDto model)
    {
        if (ModelState.IsValid)
        {
            ResponseDto? response = await _couponService.CreateCouponAsync(model);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon created successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Delete(int couponId)
    {
        ResponseDto? response = await _couponService.GetCouponByIdAsync(couponId);

        if (response != null && response.IsSuccess)
        {
            CouponDto? model = JsonSerializer.Deserialize<CouponDto>(Convert.ToString(response.Result)!, _propertyCase)!;
            return View(model);
        }
        else{
            TempData["error"] = response?.Message;
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(CouponDto couponDto)
    {
        ResponseDto? response = await _couponService.DeleteCouponAsync(couponDto.CouponId);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Coupon deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(couponDto);
    }

}