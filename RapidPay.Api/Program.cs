using Microsoft.EntityFrameworkCore;
using RapidPay.Api.Framework;
using RapidPay.DataAccess.Sql;
using RapidPay.Domain.Adapters;
using RapidPay.Domain.Repository;
using RapidPay.Domain.Services;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        new RapidWebApi()
            .ConfigureAndRun(args,
                builderConfiguredAction: builder =>
                {
                    builder.Services.AddDbContext<CardsManagementDbContext>(options =>
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
                        .AddScoped<ICardsManagementRepository, CardsManagementSqlRepository>() //.AddSingleton<ICardsManagementRepository, CardsManagementInMemoryRepository>()
                        .AddTransient<IPaymentFeesAdapter, PaymentFeesApiClient>() //.AddSingleton<IPaymentFeesAdapter>(RandomPaymentFeesManager.Instance)
                        .AddTransient<ICardsManager, CardsManager>();
                });
    }
}