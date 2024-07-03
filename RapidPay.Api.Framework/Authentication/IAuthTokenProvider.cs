namespace RapidPay.Api.Framework.Authentication
{
    public interface IAuthTokenProvider
    {
        string? GetAuthToken();
        void SetAuthToken(string? token);
        void ResetAuthToken();
    }
}