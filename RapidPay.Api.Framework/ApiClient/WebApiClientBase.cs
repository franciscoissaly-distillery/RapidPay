
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RapidPay.Api.Framework.Authentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static RapidPay.Api.Framework.ApiClient.WebApiClientBase;

namespace RapidPay.Api.Framework.ApiClient
{
    public abstract class WebApiClientBase
    {
        protected readonly IHttpClientFactory _clientFactory;
        protected readonly IAuthTokenProvider _tokenProvider;
        protected readonly IConfiguration _configuration;

        protected WebApiClientBase(IHttpClientFactory clientFactory, IAuthTokenProvider tokenProvider, IConfiguration configuration)
        {
            this._clientFactory = clientFactory;
            this._tokenProvider = tokenProvider;
            this._configuration = configuration;
        }

        protected async Task<TResponse?> InvokeGet<TResponse, TRequest>(string relativeUrl, TRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            HttpClient client = GetConfiguredHttpClient(requestMessage);

            var responseMessage = await client.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadFromJsonAsync<TResponse>();
            return response;
        }

        protected virtual HttpClient GetConfiguredHttpClient(HttpRequestMessage request)
        {
            HttpClient client = CreateHttpClient();

            if (client.BaseAddress is null)
                client.BaseAddress = GetBaseAddressFromConfiguration();

            if (!client.DefaultRequestHeaders.Accept.Any())
                client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (client.DefaultRequestHeaders.Authorization is null)
                client.DefaultRequestHeaders.Authorization = GetAuthorizationHeader();

            foreach (var header in client.DefaultRequestHeaders)
                request.Headers.Add(header.Key, header.Value);

            return client;
        }

        protected virtual HttpClient CreateHttpClient()
        {
            HttpClient client = this._clientFactory.CreateClient(GetClientName());
            return client;
        }

        protected virtual string GetClientName()
        {
            return this.GetType().Name;
        }


        protected virtual Uri? GetBaseAddressFromConfiguration()
        {
            var externalEndpoints = ExternalApisSettings.ReadFromConfiguration(_configuration);
            if (externalEndpoints is null)
                throw new InvalidOperationException("Invalid external API configuration");

            var apiName = GetClientName();

            var targetEndpoint = externalEndpoints.GetByName(apiName);
            if (targetEndpoint is null)
                throw new InvalidOperationException($"Missing external API configuration for '{apiName}'");

            string url = targetEndpoint.BaseUrl;
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException($"Invalid configuration for API '{apiName}': Empty URL");

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new InvalidOperationException($"Invalid configuration for API '{apiName}': Bad URL '{url}'");

            var uri = new Uri(url, UriKind.Absolute);
            if (!uri.LocalPath.EndsWith("api"))
                uri = new Uri(uri, new Uri("api/", UriKind.Relative));

            return uri;
        }

        protected virtual AuthenticationHeaderValue? GetAuthorizationHeader()
        {
            var token = _tokenProvider.GetAuthToken();
            if (token is null)
                return null;
            else
                return new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
