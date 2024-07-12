using RapidPay.Cards.Domain.Entities;
using RapidPay.Domain.Entities;

namespace RapidPay.Cards.Domain.Repository
{
    public interface ICardsRepository
    {
        Task<CardTransactionType?> GetTransactionType(string systemCode);
        Task<Card?> GetCard(string cardNumber);
        Task<bool> SaveCard(Card card);
        Task<CardTransaction?> GetCardLastTransaction(Card existingCard, DateTime? asOfDate = default);
        Task<bool> SaveTransaction(CardTransaction transaction);

        Task<List<Card>> GetAllCards();
        Task<List<CardTransaction>> GetAllCardTransactions(Card existingCard, DateTime? asOfDate = default);
        Task<decimal> GetBalanceAmountFromLastTransaction(Card existingCard, DateTime? asOfDate = null);

        Task<bool> DeleteCard(Card card);
    }
}
