using System.Text.Json;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;

namespace Mango.Services.ShoppingCartAPI.Service;

public class CouponService : ICouponService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions _propertyCase = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CouponService(IHttpClientFactory clientFactory)
    {
        _httpClientFactory = clientFactory;
    }

    public async Task<CouponDto> GetCoupon(string couponCode)
    {
        var client = _httpClientFactory.CreateClient("Coupon");
        var response = await client.GetAsync($"/api/coupon/GetByCode/{couponCode}");
        var apiContent = await response.Content.ReadAsStringAsync();
        var resp = JsonSerializer.Deserialize<ResponseDto>(apiContent, _propertyCase);
        if (resp!=null && resp.IsSuccess)
        {
            return JsonSerializer.Deserialize<CouponDto>(Convert.ToString(resp.Result),_propertyCase);
        }

        return new();
    }
}