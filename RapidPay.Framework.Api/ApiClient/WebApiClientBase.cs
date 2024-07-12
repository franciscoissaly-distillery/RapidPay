using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RapidPay.Framework.Api.Authentication;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace RapidPay.Framework.Api.ApiClient
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

        protected async Task<TResponse> InvokeGet<TResponse>(string relativeUrl)
        {
            return await InvokeHttp<TResponse>(HttpMethod.Get, relativeUrl);
        }

        protected async Task<TResponse> InvokeGet<TResponse, TRequest>(string relativeUrl, TRequest request)
        {
            if (request is null)
                return await this.InvokeGet<TResponse>(relativeUrl);

            Type requestType = typeof(TRequest);
            if (requestType.IsValueType || requestType == typeof(string))
                return await this.InvokeGetByKey<TResponse, TRequest>(relativeUrl, request);

            return await InvokeHttp<TResponse, object, TRequest>(HttpMethod.Get, relativeUrl, key: request);
        }

        protected async Task<TResponse> InvokeGetByKey<TResponse>(string relativeUrl, object key)
        {
            return await InvokeGetByKey<TResponse, object>(relativeUrl, key);
        }

        protected async Task<TResponse> InvokeGetByKey<TResponse, TKey>(string relativeUrl, TKey key)
        {
            if (key is null)
                return await this.InvokeGet<TResponse>(relativeUrl);

            Type keyType = typeof(TKey);
            if (keyType.IsValueType || keyType == typeof(string))
                return await InvokeHttp<TResponse, object, TKey>(HttpMethod.Get, relativeUrl, key: key);

            else
                return await InvokeGet<TResponse, TKey>(relativeUrl, key);
        }


        protected async Task<TResponse> InvokePost<TResponse, TRequest>(string relativeUrl, TRequest request)
        {
            return await InvokeHttp<TResponse, TRequest>(HttpMethod.Post, relativeUrl, request);
        }

        protected async Task<TResponse> InvokePostByKey<TResponse, TRequest>(string relativeUrl, object key, TRequest request)
        {
            return await InvokePostByKey<TResponse, TRequest, object>(relativeUrl, key, request);
        }

        protected async Task<TResponse> InvokePostByKey<TResponse, TRequest, TKey>(string relativeUrl, TKey key, TRequest request)
        {
            return await InvokeHttp<TResponse, TRequest, TKey>(HttpMethod.Post, relativeUrl, request, key);
        }


        protected async Task<TResponse> InvokePut<TResponse, TRequest, TKey>(string relativeUrl, TKey key, TRequest request)
        {
            return await InvokeHttp<TResponse, TRequest, TKey>(HttpMethod.Post, relativeUrl, request, key);
        }


        protected async Task<TResponse> InvokeDelete<TResponse, TRequest>(string relativeUrl, TRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Type requestType = typeof(TRequest);
            if (requestType.IsValueType || requestType == typeof(string))
                return await InvokeDeleteByKey<TResponse, TRequest>(relativeUrl, request);

            return await InvokeHttp<TResponse, TRequest>(HttpMethod.Delete, relativeUrl, request);
        }

        protected async Task<TResponse> InvokeDeleteByKey<TResponse, TKey>(string relativeUrl, TKey key)
        {
            ArgumentNullException.ThrowIfNull(key);

            Type keyType = typeof(TKey);
            if (keyType.IsValueType || keyType == typeof(string))
                return await InvokeHttp<TResponse, object, TKey>(HttpMethod.Delete, relativeUrl, key: key);

            else
                return await InvokeDelete<TResponse, TKey>(relativeUrl, key);
        }



        protected async Task<TResponse> InvokeHttp<TResponse, TRequest>(HttpMethod verb, string relativeUrl, TRequest request, Action<HttpRequestMessage, TRequest>? requestAction = null)
        {
            if (requestAction is null)
                return await this.InvokeHttp<TResponse, TRequest, object>(verb, relativeUrl, request);
            else
                return await this.InvokeHttp<TResponse, TRequest, object>(verb, relativeUrl, new RequestMessageValue<TRequest>(request) { Action = requestAction });
        }

        protected async Task<TResponse> InvokeHttp<TResponse>(HttpMethod verb, string relativeUrl)
        {
            return await this.InvokeHttp<TResponse, object, object>(verb, relativeUrl);
        }

        protected async Task InvokeHttp(HttpMethod verb, string relativeUrl)
        {
            await this.InvokeHttp<object, object, object>(verb, relativeUrl);
        }


        protected class RequestMessageValue<TValue>
        {
            public TValue? Value { get; }

            public Action<HttpRequestMessage, TValue>? Action { get; init; }

            public RequestMessageValue(TValue? value)
            {
                Value = value;
            }

            public static implicit operator RequestMessageValue<TValue>(TValue value)
            {
                return new RequestMessageValue<TValue>(value);
            }
        }

        protected readonly JsonSerializerOptions _responseJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        protected async Task<TResponse> InvokeHttp<TResponse, TRequest, TKey>(HttpMethod verb, string relativeUrl,
                                                    RequestMessageValue<TRequest>? request = null,
                                                    RequestMessageValue<TKey>? key = null)
        {
            ArgumentNullException.ThrowIfNull(verb);
            ArgumentException.ThrowIfNullOrWhiteSpace(relativeUrl);

            HttpClient client = GetConfiguredHttpClient();
            if (client.BaseAddress is null)
                throw new InvalidOperationException("Invalid null base address to invoke");

            var url = new Uri(client.BaseAddress, new Uri(relativeUrl, UriKind.Relative));
            var requestMessage = new HttpRequestMessage(verb, url);

            if (key is not null && key.Value is not null)
            {
                var keyAction = key.Action;
                if (keyAction is null)
                    keyAction = PopulateUrl;

                if (keyAction is not null)
                    keyAction.Invoke(requestMessage, key.Value);
            }

            if (request is not null && request.Value is not null)
            {
                var requestAction = request.Action;
                if (requestAction is null)
                    if (verb == HttpMethod.Get)
                        requestAction = PopulateQueryString;
                    else if (verb != HttpMethod.Delete)
                        requestAction = PopulateBody;

                if (requestAction is not null)
                    requestAction.Invoke(requestMessage, request.Value);
            }

            foreach (var header in client.DefaultRequestHeaders)
                requestMessage.Headers.Add(header.Key, header.Value);

            var responseMessage = await client.SendAsync(requestMessage);
            HttpRequestException? httpRequestException = null;
            try
            {
                responseMessage.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                httpRequestException = ex;
            }

            if (httpRequestException != null)
            {
                if (!responseMessage.IsSuccessStatusCode && IsClientError(responseMessage.StatusCode))
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        var sb = new StringBuilder(httpRequestException.Message);
                        ValidationProblemDetails? validationProblemDetails = null;
                        try
                        {
                            validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, _responseJsonOptions);
                        }
                        catch (Exception)
                        {
                            sb.Append(Environment.NewLine)
                              .Append($"   - Content: {responseContent}");
                        }

                        if (validationProblemDetails is not null)
                            foreach (var error in validationProblemDetails.Errors)
                                sb.Append(Environment.NewLine)
                                  .Append($"   - {error.Key}: {string.Join(";", error.Value)}");

                        httpRequestException = new HttpRequestException(sb.ToString(), httpRequestException, httpRequestException.StatusCode);
                    }
                }
                throw httpRequestException;
            }


            var response = default(TResponse);

            if (typeof(TResponse) == typeof(object))
                return response!;

            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                return response!;

            var responseContentString = await responseMessage.Content.ReadAsStringAsync();
            try
            {
                response = JsonSerializer.Deserialize<TResponse>(responseContentString, _responseJsonOptions);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder(ex.Message);
                sb.Append(Environment.NewLine).Append($"Unable to create a '{typeof(TResponse).Name}' response from content '{responseContentString}'");
                throw new HttpRequestException(sb.ToString(), ex, HttpStatusCode.InternalServerError);
            }

            return response!;
        }

        protected static bool IsClientError(HttpStatusCode statusCode)
        {
            return statusCode >= HttpStatusCode.BadRequest && statusCode < HttpStatusCode.InternalServerError;
        }

        protected static bool IsServerError(HttpStatusCode statusCode)
        {
            return statusCode >= HttpStatusCode.InternalServerError;
        }

        protected void PopulateUrl<TKey>(HttpRequestMessage requestMessage, TKey key)
        {
            ArgumentNullException.ThrowIfNull(requestMessage);
            if (requestMessage.RequestUri is null)
                throw new ArgumentException("Invalid request message with null RequestUri", nameof(requestMessage));

            if (key is null)
                return;

            var requestUrl = requestMessage.RequestUri.ToString();
            string? newUrl;
            if (typeof(TKey).IsValueType || typeof(TKey) == typeof(string))
                newUrl = BuildUrlAppendingValue(requestUrl, key);
            else
                newUrl = BuildUrlFromPropertyValues(requestUrl, key);

            if (requestUrl != newUrl)
                requestMessage.RequestUri = new Uri(newUrl!);
        }

        protected virtual string? BuildUrlAppendingValue<TValue>(string? urlTemplate, TValue value)
        {
            return BuildUrlWithValue(urlTemplate, value, placeholder: null);
        }

        protected virtual string? BuildUrlFromPropertyValues<TRequest>(string? urlTemplate, TRequest request)
        {
            var propertyValues = ExtractPropertyValues(request);
            return BuildUrlReplacingPlaceholders(urlTemplate, propertyValues);
        }

        protected virtual Dictionary<string, object?> ExtractPropertyValues<TRequest>(TRequest? request)
        {
            if (typeof(TRequest).IsValueType || typeof(TRequest) == typeof(string))
                throw new ArgumentException($"Invalid request type '{typeof(TRequest).Name}'. Expected a complex object", nameof(request));

            if (request is Dictionary<string, object?> targetDictionary)
                return targetDictionary;

            var propertyValues = new Dictionary<string, object?>();
            if (request is null)
                return propertyValues;

            var properties = request.GetType()
                                    .GetProperties()
                                    .Where(x => x.CanRead)
                                    .Where(x => x.GetIndexParameters().Length == 0)
                                    .Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>(true) is null)
                                    .ToList();

            foreach (var property in properties)
                try
                {
                    var propertyValue = property.GetValue(request);
                    if (propertyValue is not null)
                        propertyValues.Add(property.Name, propertyValue);
                }
                catch { } //ignore problematic getters

            return propertyValues;
        }

        protected virtual string? BuildUrlReplacingPlaceholders(string? urlTemplate, Dictionary<string, object?> valuePairs)
        {
            if (string.IsNullOrWhiteSpace(urlTemplate))
                return null;

            if (valuePairs is not null)
                foreach (var pair in valuePairs)
                    urlTemplate = BuildUrlWithValue(urlTemplate, pair.Value, pair.Key);

            return urlTemplate;
        }

        protected virtual string? BuildUrlWithValue<TValue>(string? urlTemplate, TValue value, string? placeholder)
        {
            if (value is not null)
                if (value.ToString() is string valueString)
                    if (!string.IsNullOrWhiteSpace(valueString))
                        if (valueString != value.GetType().FullName) // not default object.ToString()
                        {
                            if (urlTemplate is null)
                                urlTemplate = string.Empty;

                            var encodedString = HttpUtility.UrlEncode(valueString);
                            if (string.IsNullOrWhiteSpace(placeholder))
                            {
                                if (urlTemplate.Length > 0 && !urlTemplate.EndsWith("/"))
                                    urlTemplate += "/";
                                urlTemplate += encodedString;
                            }
                            else
                                urlTemplate = urlTemplate.Replace("{" + placeholder + "}", encodedString, StringComparison.OrdinalIgnoreCase);
                        }

            return urlTemplate;
        }


        protected void PopulateQueryString<TRequest>(HttpRequestMessage requestMessage, TRequest request)
        {
            ArgumentNullException.ThrowIfNull(requestMessage);
            if (requestMessage.RequestUri is null)
                throw new ArgumentException("Invalid request message with null RequestUri", nameof(requestMessage));

            var propertyValues = ExtractPropertyValues(request);
            var queryString = BuildQueryString(propertyValues);
            if (string.IsNullOrEmpty(queryString))
                return;

            var uriBuilder = new UriBuilder(requestMessage.RequestUri);
            if (string.IsNullOrWhiteSpace(uriBuilder.Query))
                uriBuilder.Query = queryString;
            else
                uriBuilder.Query = $"{uriBuilder.Query.Substring(1)}&{queryString}";

            requestMessage.RequestUri = uriBuilder.Uri;
        }

        protected virtual string? BuildQueryString(Dictionary<string, object?> valuePairs)
        {
            if (valuePairs is null)
                return null;

            var valueStrings = from pair in valuePairs
                               where pair.Value is not null
                               select $"{pair.Key}={HttpUtility.UrlEncode(pair.Value.ToString())}";

            var queryString = string.Join("&", valueStrings);
            return queryString;
        }


        protected void PopulateBody<TRequest>(HttpRequestMessage requestMessage, TRequest request)
        {
            if (request is null)
                return;

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

            var url = targetEndpoint.BaseUrl;
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
