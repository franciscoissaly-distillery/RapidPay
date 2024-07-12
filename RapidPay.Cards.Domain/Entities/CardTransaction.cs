using RapidPay.Cards.Domain.Entities;

namespace RapidPay.Domain.Entities
{
    public partial class CardTransaction
    {
        public CardTransaction(Card card)
        {
            ArgumentNullException.ThrowIfNull(card);
            Card = card;
        }

        public Card Card { get; }

        public Guid Id { get; set; }

        public required CardTransactionType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public decimal TransactionAmount { get; set; } = 0;
        public decimal FeeAmount { get; set; } = 0;
        public decimal CardBalanceAmount { get; set; } = 0;
    }
}
