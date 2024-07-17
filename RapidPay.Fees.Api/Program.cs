using RapidPay.Cards.Adapters.Fees;
using RapidPay.Fees.Domain.Mocks;
using RapidPay.Framework.Api.Setup;

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