namespace RapidPay.Auth.Api.Logic
{
    public interface IJwtTokenService
    {
        string GenerateToken(string username);
    }
}