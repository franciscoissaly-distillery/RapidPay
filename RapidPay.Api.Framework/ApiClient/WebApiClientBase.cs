
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RapidPay.Api.Framework.Authentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Web;
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
            return await InvokeHttp<TResponse, TRequest>(HttpMethod.Get, relativeUrl, request, PopulateQueryString);
        }

        protected async Task<TResponse?> InvokePost<TResponse, TRequest>(string relativeUrl, TRequest request)
        {
            return await InvokeHttp<TResponse, TRequest>(HttpMethod.Post, relativeUrl, request, PopulateBody);
        }

        protected async Task<TResponse?> InvokeHttp<TResponse, TRequest>(
            HttpMethod verb,
            string relativeUrl,
            TRequest request,
            Action<HttpRequestMessage, TRequest> populateRequestMessageAction)
        {
            ArgumentNullException.ThrowIfNull(verb);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(relativeUrl);

            HttpClient client = GetConfiguredHttpClient();
            if (client.BaseAddress is null)
                throw new InvalidOperationException("Invalid null base address to invoke");

            var url = new Uri(client.BaseAddress, new Uri(relativeUrl, UriKind.Relative));
            var requestMessage = new HttpRequestMessage(verb, url);

            if (populateRequestMessageAction is not null)
                if (request is not null)
                    populateRequestMessageAction.Invoke(requestMessage, request);

            foreach (var header in client.DefaultRequestHeaders)
                requestMessage.Headers.Add(header.Key, header.Value);

            var responseMessage = await client.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadFromJsonAsync<TResponse>();
            return response;
        }

        protected void PopulateQueryString<TRequest>(HttpRequestMessage requestMessage, TRequest request)
        {
            ArgumentNullException.ThrowIfNull(requestMessage);
            if (requestMessage.RequestUri is null)
                throw new ArgumentException("Invalid request message with null RequestUri", nameof(requestMessage));

            var queryString = BuildQueryString(request);
            if (string.IsNullOrEmpty(queryString))
                return;

            var uriBuilder = new UriBuilder(requestMessage.RequestUri);
            if (string.IsNullOrWhiteSpace(uriBuilder.Query))
                uriBuilder.Query = queryString;
            else
                uriBuilder.Query = $"{uriBuilder.Query.Substring(1)}&{queryString}";

            requestMessage.RequestUri = uriBuilder.Uri;
        }

        public virtual string? BuildQueryString<TRequest>(TRequest? request)
        {
            if (request is null)
                return null;

            var properties = (
                                from eachProperty in request.GetType().GetProperties()
                                where eachProperty.CanRead
                                where eachProperty.GetIndexParameters().Length == 0
                                where eachProperty.GetCustomAttribute<JsonIgnoreAttribute>(true) is null
                                let eachPropertyValue = eachProperty.GetValue(request)
                                where eachPropertyValue is not null
                                select eachProperty.Name + "=" + HttpUtility.UrlEncode(eachPropertyValue.ToString())
                             ).ToArray();

            string queryString = string.Join("&", properties);
            return queryString;
        }


        protected void PopulateBody<TRequest>(HttpRequestMessage requestMessage, TRequest request)
        {
            requestMessage.Content = JsonContent.Create(request);
        }


        protected virtual HttpClient GetConfiguredHttpClient()
        {
            HttpClient client = CreateHttpClient();

            if (client.BaseAddress is null)
                client.BaseAddress = GetBaseAddressFromConfiguration();

            if (!client.DefaultRequestHeaders.Accept.Any())
                client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (client.DefaultRequestHeaders.Authorization is null)
                client.DefaultRequestHeaders.Authorization = GetAuthorizationHeader();

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
