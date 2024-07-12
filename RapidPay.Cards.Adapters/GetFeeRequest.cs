namespace RapidPay.Cards.Adapters
{
    public class GetFeeRequest
    {
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public decimal TransactionAmount { get; set; } = 0;
    }

}
