using RapidPay.Api.Framework;
using RapidPay.Domain.Adapters;
using RapidPay.Fees.Mocks;

internal class Program
{
    private static void Main(string[] args)
    {
        new RapidWebApi()
           .ConfigureAndRun(
               args,
               builder =>
               {
                   builder.Services.AddSingleton<IPaymentFeesAdapter>(RandomPaymentFeesManager.Instance);
               }
           );
    }
}