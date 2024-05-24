using RapidPay.Domain.Entities;
using System.Transactions;

namespace RapidPay.Domain.Repository
{
    public interface ICardsManagementRepository
    {
        Card GetCardByNumber(string cardNumber);
        bool SaveCard(Card card);
        CardTransaction GetCardLastTransaction(Card existingCard, DateTime? asOfDate = default);
        bool SaveTransaction(CardTransaction transaction);

        IEnumerable<Card> GetAllCards();
        IEnumerable<CardTransaction> GetAllCardTransactions(Card existingCard, DateTime? asOfDate = default);
    }
}
