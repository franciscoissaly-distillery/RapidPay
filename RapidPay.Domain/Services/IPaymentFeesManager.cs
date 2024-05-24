using RapidPay.Domain.Entities;

namespace RapidPay.Domain.Services
{
    public interface IPaymentFeesManager
    {
        decimal CalculatePaymentFee(CardTransaction payment);
    }
}
