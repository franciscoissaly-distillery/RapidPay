using RapidPay.Domain.Entities;

namespace RapidPay.Domain.Services
{
    public interface ICardsManager
    {
        Card CreateCard(string cardNumber);

        Card GetCard(string cardNumber);
        CardTransaction RegisterCardPayment(string cardNumber, decimal paymentAmount);
        decimal GetCardBalance(string cardNumber, DateTime? asOfDate = default);

        bool IsValidCardNumber(string cardNumber);
        IEnumerable<Card> GetAllExistingCards();
        IEnumerable<CardTransaction> GetCardTransactions(string cardNumber);
    }
}
