using RapidPay.Domain.Entities;

namespace RapidPay.Domain.Services
{
    public interface IPaymentFeesManager
    {
        Task<decimal> CalculatePaymentFee(CardTransaction payment);
    }
}
