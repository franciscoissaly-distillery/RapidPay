using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RapidPay.Api.Framework.Authentication;
using RapidPay.Auth.Api.Models;

namespace RapidPay.Api.Test
{
    [TestFixture]
    public abstract class ApiTestFixtureBase
    {
        private ServiceProvider _serviceProvider;

        [OneTimeSetUp]
        public async Task FixtureSetUp()
        {
            var serviceCollection = new ServiceCollection();

            // Set up configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddScoped<IAuthTokenProvider, AuthTokenHolder>();
            serviceCollection.AddHttpClient<AuthApiClient>();
            serviceCollection.AddTransient<AuthApiClient>();

            OnAddServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
            await OnFixtureSetUp();
        }
        protected virtual void OnAddServices(IServiceCollection serviceCollection) { }
        protected virtual async Task OnFixtureSetUp() { }

        [OneTimeTearDown]
        public async Task FixtureCleanUp()
        {
            await OnFixtureCleanUp();
            LogoutTestUser();
            await _serviceProvider.DisposeAsync();
        }
        protected virtual async Task OnFixtureCleanUp() { }


        [SetUp]
        public async Task TestSetup()
        {
            await OnTestSetUp();
        }
        protected virtual async Task OnTestSetUp() { }


        [TearDown]
        public async Task TestCleanUp()
        {
            await OnTestCleanUp();
        }
        protected virtual async Task OnTestCleanUp() { }


        protected TService GetService<TService>()
        {
            if (_serviceProvider is null)
                throw new InvalidOperationException($"The fixture class has not been initialized. Invoke '{nameof(FixtureSetUp)}' first.");

            var service = _serviceProvider.GetService<TService>();
            Assert.That(service, Is.Not.Null);
            return service;
        }

        protected async Task<bool> LoginTestUser()
        {
            var tokenProvider = GetService<IAuthTokenProvider>();
            Assert.That(tokenProvider, Is.Not.Null);

            var token = tokenProvider.GetAuthToken();
            if (token is not null)
                return true;

            var apiClient = GetService<AuthApiClient>();
            Assert.That(apiClient, Is.Not.Null);

            var loginRequest = OnGetLoginCredentials();
            Assert.That(loginRequest, Is.Not.Null);
            Assert.That(loginRequest.Username, Is.Not.Empty);
            Assert.That(loginRequest.Password, Is.Not.Empty);

            var loginResponse = await apiClient.Login(loginRequest);
            Assert.That(loginResponse, Is.Not.Null);
            Assert.That(loginResponse.Token, Is.Not.Null);
            Assert.That(loginResponse.Token, Is.Not.Empty);

            tokenProvider.SetAuthToken(loginResponse.Token);
            token = tokenProvider.GetAuthToken();
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.EqualTo(loginResponse.Token));

            return true;
        }

        protected virtual LoginRequest OnGetLoginCredentials()
        {
            return new LoginRequest() { Username = "testuser", Password = "testpassword" };
        }

        protected void LogoutTestUser()
        {
            var tokenProvider = GetService<IAuthTokenProvider>();
            Assert.That(tokenProvider, Is.Not.Null);

            tokenProvider.ResetAuthToken();
            var token = tokenProvider.GetAuthToken();
            Assert.That(token, Is.Null);
        }
    }
}