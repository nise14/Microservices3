using static Mango.Web.Utility.SD;

namespace Mango.Web.Models;

public class RequestDto
{
    public ApiType ApiType { get; set; } = ApiType.GET;
    public string Url { get; set; } = null!;
    public object Data { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
}