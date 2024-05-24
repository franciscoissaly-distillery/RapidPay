using RapidPay.Domain.Entities;

namespace RapidPay.Api.Models
{
    public class TransactionDto
    {
        public string CardNumber { get; internal set; }
        public string TypeCode { get; set; }

        public string TypeName { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal BalanceAmount { get; set; }
    }
}
