using RapidPay.Domain.Entities;

namespace RapidPay.Domain.Services
{
    public interface ICardsManager
    {
        Task<Card> CreateCard(string cardNumber);

        Task<Card?> GetCard(string cardNumber);
        Task<Card> GetExistingCard(string cardNumber);
        Task<CardTransaction> RegisterCardPayment(string cardNumber, decimal paymentAmount);
        Task<decimal> GetCardBalance(string cardNumber, DateTime? asOfDate = default);

        bool IsValidCardNumber(string cardNumber);
        Task<IEnumerable<Card>> GetAllExistingCards();
        Task<IEnumerable<CardTransaction>> GetCardTransactions(string cardNumber);

        Task<bool> DeleteCard(string cardNumber);
    }
}
