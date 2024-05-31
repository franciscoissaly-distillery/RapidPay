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

        protected CardTransaction(string cardNumber)
        {
            ArgumentNullException.ThrowIfNull(cardNumber);
            _cardNumber = cardNumber;
        }
        private string _cardNumber;
        public string CardNumber => Card != null ? Card.Number : _cardNumber;

        #endregion


        public Card Card { get; }

        public Guid Id { get; set; }

        public CardTransactionType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal CardBalanceAmount { get; set; }
    }
}
