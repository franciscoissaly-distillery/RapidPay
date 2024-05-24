namespace RapidPay.Domain.Entities
{
    public class CardTransaction
    {
        public CardTransaction(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            Card = card;
        }

        public Card Card { get; init; }

        public CardTransactionType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal CardBalanceAmount { get; set; }
    }
}
