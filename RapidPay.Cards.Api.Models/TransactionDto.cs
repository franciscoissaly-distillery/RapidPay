namespace RapidPay.Cards.Api.Models
{
    public class TransactionDto
    {
        public required string CardNumber { get; set; }
        public required string TypeCode { get; set; }
        public required string TypeName { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal BalanceAmount { get; set; }
    }
}
