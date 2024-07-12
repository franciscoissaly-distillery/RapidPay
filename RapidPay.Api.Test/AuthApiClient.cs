using Microsoft.Extensions.Configuration;
using RapidPay.Api.Framework.ApiClient;
using RapidPay.Api.Framework.Authentication;
using RapidPay.Auth.Api.Models;

namespace RapidPay.Api.Test
{
    public class AuthApiClient : WebApiClientBase
    {

        private const string AuthUrl = "auth";

        public AuthApiClient(
                IHttpClientFactory clientFactory,
                IAuthTokenProvider tokenProvider,
                IConfiguration configuration)
                : base(clientFactory, tokenProvider, configuration)
        { }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            return await InvokePost<LoginResponse, LoginRequest>(AuthUrl + "/login", request);
        }
    }
}