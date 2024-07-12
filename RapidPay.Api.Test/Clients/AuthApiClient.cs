using Microsoft.Extensions.Configuration;
using RapidPay.Auth.Api.Models;
using RapidPay.Framework.Api.ApiClient;
using RapidPay.Framework.Api.Authentication;

namespace RapidPay.Api.Test.Clients
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