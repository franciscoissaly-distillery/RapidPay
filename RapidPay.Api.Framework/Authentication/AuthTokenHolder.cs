namespace RapidPay.Api.Framework.Authentication
{
    public class AuthTokenHolder : IAuthTokenProvider
    {
        private string? _currentToken;

        public string? GetAuthToken()
        {
            return _currentToken;
        }

        public void SetAuthToken(string? token)
        {
            _currentToken = token;
        }

        public void ResetAuthToken()
        {
            SetAuthToken(null);
        }
    }

}