namespace RapidPay.Framework.Api.Authentication
{
    public interface IAuthTokenProvider
    {
        string? GetAuthToken();
        void SetAuthToken(string? token);
        void ResetAuthToken();
    }
}