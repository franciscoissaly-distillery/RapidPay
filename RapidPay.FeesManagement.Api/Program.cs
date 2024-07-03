using RapidPay.Api.Framework;
using RapidPay.Domain.Adapters;
using RapidPay.Fees.Mocks;

internal class Program
{
    private static void Main(string[] args)
    {
        new RapidWebAPi()
           .ConfigureAndRun(
               args,
               builder =>
               {
                   builder.Services.AddSingleton<IPaymentFeesAdapter>(RandomPaymentFeesManager.Instance);
               }
           );
    }
}