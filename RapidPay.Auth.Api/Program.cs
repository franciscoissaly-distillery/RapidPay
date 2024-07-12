using RapidPay.Api.Framework;
using RapidPay.Auth.Api.Logic;
using RapidPay.Auth.Mocks;
using RapidPay.Domain.Adapters;

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