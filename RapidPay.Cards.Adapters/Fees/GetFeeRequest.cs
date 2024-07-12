namespace RapidPay.Cards.Adapters.Fees
{
    public class GetFeeRequest
    {
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public decimal TransactionAmount { get; set; } = 0;
    }

}
