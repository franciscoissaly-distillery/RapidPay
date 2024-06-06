namespace RapidPay.Domain.Entities
{
    public partial class CardTransaction //for DB mapping purposes
    {
        private CardTransaction(string cardNumber)
        {
            ArgumentNullException.ThrowIfNull(cardNumber);
            _cardNumber = cardNumber;
            Card = null!;
        }

        private string _cardNumber = null!;
        public string CardNumber
        {
            get
            {
                if (Card == null)
                    return _cardNumber;

                return Card.Number;
            }
        }
    }
}