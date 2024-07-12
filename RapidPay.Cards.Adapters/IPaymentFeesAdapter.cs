namespace RapidPay.Cards.Adapters
{
    public interface IPaymentFeesAdapter
    {
        Task<decimal> CalculatePaymentFee(GetFeeRequest request);
    }
}
