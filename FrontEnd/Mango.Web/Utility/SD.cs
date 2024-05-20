namespace Mango.Web.Utility;

public class SD
{
    public static string CouponAPIBase { get; set; } = null!;
    public static string ProductAPIBase { get; set; } = null!;
    public static string AuthAPIBase { get; set; } = null!;
    public static string ShoppingCartAPIBase{ get; set; } = null!;
    public const string ROLEADMIN = "ADMIN";
    public const string ROLECUSTOMER = "CUSTOMER";
    public const string TOKENCOOKIE = "JWTToken";

    public enum ApiType
    {
        GET,
        POST,
        PUT,
        DELETE
    }
}