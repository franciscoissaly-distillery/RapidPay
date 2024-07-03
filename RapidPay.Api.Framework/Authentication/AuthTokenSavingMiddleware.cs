using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPay.Api.Framework.Authentication
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
