using System.Text.Json;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;

namespace Mango.Services.ShoppingCartAPI.Service;

public class ProductService : IProductService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions _propertyCase = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public ProductService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<ProductDto>> GetProducts()
    {
        var client = _httpClientFactory.CreateClient("Product");
        var response = await client.GetAsync($"/api/product");
        var apiContent = await response.Content.ReadAsStringAsync();
        var resp = JsonSerializer.Deserialize<ResponseDto>(apiContent, _propertyCase);
        if (resp.IsSuccess)
        {
            return JsonSerializer.Deserialize<IEnumerable<ProductDto>>(Convert.ToString(resp.Result),_propertyCase);
        }

        return [];
    }
}