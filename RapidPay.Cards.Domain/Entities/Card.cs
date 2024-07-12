namespace RapidPay.Cards.Domain.Entities
{
    public class Card
    {
        public Card(string number)
        {
            ArgumentNullException.ThrowIfNull(number);
            Number = number;
        }

        public string Number { get; }
    }
}
