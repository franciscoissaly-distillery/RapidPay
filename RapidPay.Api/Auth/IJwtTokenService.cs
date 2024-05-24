namespace RapidPay.Api.Auth
{
    public interface IJwtTokenService
    {
        string GenerateToken(string username);
    }
}