using RapidPay.Domain.Entities;

namespace RapidPay.Domain.Repository
{
    public interface ICardsManagementRepository
    {
        Task<CardTransactionType?> GetTransactionType(string systemCode);
        Task<Card?> GetCard(string cardNumber);
        Task<bool> SaveCard(Card card);
        Task<CardTransaction?> GetCardLastTransaction(Card existingCard, DateTime? asOfDate = default);
        Task<bool> SaveTransaction(CardTransaction transaction);

        Task<List<Card>> GetAllCards();
        Task<List<CardTransaction>> GetAllCardTransactions(Card existingCard, DateTime? asOfDate = default);
        Task<decimal> GetBalanceAmountFromLastTransaction(Card existingCard, DateTime? asOfDate = null);
    }
}
