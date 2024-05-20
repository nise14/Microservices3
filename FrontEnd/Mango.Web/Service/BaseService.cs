using System.Net;
using System.Text;
using System.Text.Json;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Service;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenProvider _tokenProvider;
    private static readonly JsonSerializerOptions _propertyCase = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
    }

    public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient("MangoAPI");
            HttpRequestMessage message = new();
            message.Headers.Add("Accept", "application/json");

            if (withBearer)
            {
                var token = _tokenProvider.GetToken();
                message.Headers.Add("Authorization", $"Bearer {token}");
            }

            message.RequestUri = new Uri(requestDto.Url);
            if (requestDto.Data is not null)
            {
                message.Content = new StringContent(JsonSerializer.Serialize(requestDto.Data, _propertyCase), Encoding.UTF8, "application/json");
            }

            switch (requestDto.ApiType)
            {
                case ApiType.POST:
                    message.Method = HttpMethod.Post;
                    break;
                case ApiType.DELETE:
                    message.Method = HttpMethod.Delete;
                    break;
                case ApiType.PUT:
                    message.Method = HttpMethod.Put;
                    break;
                default:
                    message.Method = HttpMethod.Get;
                    break;
            }

            HttpResponseMessage? apiResponse = await client.SendAsync(message);

            switch (apiResponse?.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new()
                    {
                        IsSuccess = false,
                        Message = "Not Found"
                    };
                case HttpStatusCode.Forbidden:
                    return new()
                    {
                        IsSuccess = false,
                        Message = "Access Denied"
                    };
                case HttpStatusCode.Unauthorized:
                    return new()
                    {
                        IsSuccess = false,
                        Message = "Unauthorized"
                    };
                case HttpStatusCode.InternalServerError:
                    return new()
                    {
                        IsSuccess = false,
                        Message = "Internal Server Error"
                    };
                default:
                    var apiContent = await apiResponse?.Content.ReadAsStringAsync()!;

                    var apiReponseDto = JsonSerializer.Deserialize<ResponseDto>(apiContent, _propertyCase);
                    return apiReponseDto;
            }
        }
        catch (Exception ex)
        {
            var dto = new ResponseDto
            {
                Message = ex.ToString(),
                IsSuccess = false
            };
            return dto;
        }
    }
}