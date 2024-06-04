namespace RapidPay.Domain.Entities
{
    public class CardTransaction
    {

        public CardTransaction(Card card)
        {
            ArgumentNullException.ThrowIfNull(card);
            Card = card;
        }

        #region "for db mapping purposes"

        private CardTransaction(string cardNumber)
        {
            ArgumentNullException.ThrowIfNull(cardNumber);
            _cardNumber = cardNumber;
        }

        private string _cardNumber = string.Empty;
        public string CardNumber
        {
            get
            {
                if (Card == null)
                    return _cardNumber;

                return Card.Number;
            }
        }

        #endregion


        public Card Card { get; }

        public Guid Id { get; set; }

        public CardTransactionType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public decimal TransactionAmount { get; set; } = 0;
        public decimal FeeAmount { get; set; } = 0;
        public decimal CardBalanceAmount { get; set; } = 0;
    }
}
