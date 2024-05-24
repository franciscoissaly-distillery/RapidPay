using RapidPay.Domain.Entities;
using System.Transactions;

namespace RapidPay.Domain.Repository
{
    public interface ICardsManagementRepository
    {
        Task<Card> GetCardByNumber(string cardNumber);
        Task<bool> SaveCard(Card card);
        Task<CardTransaction> GetCardLastTransaction(Card existingCard, DateTime? asOfDate = default);
        Task<bool> SaveTransaction(CardTransaction transaction);

        Task<IEnumerable<Card>> GetAllCards();
        Task<IEnumerable<CardTransaction>> GetAllCardTransactions(Card existingCard, DateTime? asOfDate = default);
    }
}
