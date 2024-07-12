namespace RapidPay.Cards.Adapters.Fees
{
    public interface IPaymentFeesAdapter
    {
        Task<decimal> CalculatePaymentFee(GetFeeRequest request);
    }
}
