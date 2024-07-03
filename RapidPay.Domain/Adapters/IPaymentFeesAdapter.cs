using RapidPay.Domain.Entities;

namespace RapidPay.Domain.Adapters
{
    public interface IPaymentFeesAdapter
    {
        Task<decimal> CalculatePaymentFee(GetFeeRequest request);
    }
}
