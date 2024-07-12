using Microsoft.EntityFrameworkCore;
using RapidPay.Cards.Adapters.Fees;
using RapidPay.Cards.Data.Sql;
using RapidPay.Cards.Domain.Repository;
using RapidPay.Cards.Domain.Services;
using RapidPay.Framework.Api;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        new RapidWebApi()
            .ConfigureAndRun(args,
                builderConfiguredAction: builder =>
                {
                    builder.Services.AddDbContext<CardsDbContext>(options =>
                    {
                        options.UseSqlServer(builder.Configuration.GetConnectionString("RapidPay"), options =>
                        {
                            var asmName = Assembly.GetExecutingAssembly().FullName;
                            options.MigrationsAssembly(asmName);
                        });
                        options.EnableSensitiveDataLogging(); // Enable parameter value logging
                        options.LogTo(Console.WriteLine, LogLevel.Information); // Log SQL to the console
                    });

                    builder.Services.AddHttpClient<PaymentFeesApiClient>();

                    // Add services to the container.
                    builder.Services
                        .AddSingleton<DefaultEntities>()
                        .AddScoped<ICardsRepository, CardsSqlRepository>() //.AddSingleton<ICardsManagementRepository, CardsManagementInMemoryRepository>()
                        .AddTransient<IPaymentFeesAdapter, PaymentFeesApiClient>() //.AddSingleton<IPaymentFeesAdapter>(RandomPaymentFeesManager.Instance)
                        .AddTransient<ICardsManager, CardsManager>();
                });
    }
}