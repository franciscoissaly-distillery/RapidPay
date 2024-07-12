using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace RapidPay.Framework.Api.Authentication
{
    public class AuthTokenSavingMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthTokenSavingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthTokenProvider tokenProvider)
        {
            if (context.User.Identity is not null
                && context.User.Identity.IsAuthenticated)
            {
                var token = await context.GetTokenAsync("access_token");
                tokenProvider.SetAuthToken(token);
            }
            else
                tokenProvider.ResetAuthToken();

            await _next(context);
        }
    }
}
