using RapidPay.Auth.Api.Logic;
using RapidPay.Auth.Domain.Mocks;
using RapidPay.Cards.Adapters;
using RapidPay.Framework.Api;

internal class Program
{
    private static void Main(string[] args)
    {
        new RapidWebApi()
            .ConfigureAndRun(
                args,
                builder =>
                {
                    builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
                    builder.Services.AddSingleton<IUsersAdapter>(new TestUsersManager());
                }
            );
    }
}