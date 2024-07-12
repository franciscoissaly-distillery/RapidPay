using RapidPay.Cards.Adapters;
using RapidPay.Fees.Domain.Mocks;
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
                   builder.Services.AddSingleton<IPaymentFeesAdapter>(RandomPaymentFeesManager.Instance);
               }
           );
    }
}