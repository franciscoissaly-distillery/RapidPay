using RapidPay.Domain.Entities;
using RapidPay.Domain.Repository;
using System.Collections.Concurrent;
using System.Linq;

namespace RapidPay.DataAccess.Mocks
{
    public class CardsManagementInMemoryRepository : ICardsManagementRepository
    {
        private readonly ConcurrentDictionary<string, Card> _cards = new();
        private readonly ConcurrentDictionary<Card, ConcurrentBag<CardTransaction>> _transactions = new();

        async public Task<IEnumerable<Card>> GetAllCards()
        {
            return await Task.FromResult<IEnumerable<Card>>(_cards.Values);
        }

        async public Task<Card> GetCardByNumber(string cardNumber)
        {
            Card result = null;
            if (!string.IsNullOrWhiteSpace(cardNumber)
                && _cards.TryGetValue(cardNumber, out var card))
                result = card;
            return await Task.FromResult(result);
        }

        async public Task<CardTransaction> GetCardLastTransaction(Card existingCard, DateTime? asOfDate = default)
        {
            var cardTransactions = await OnGetAllCardTransactions(existingCard, asOfDate, 1);
            return cardTransactions.FirstOrDefault();
        }

        async public Task<IEnumerable<CardTransaction>> GetAllCardTransactions(Card existingCard, DateTime? asOfDate = default)
        {
            return await OnGetAllCardTransactions(existingCard, asOfDate);
        }

        async private Task<IEnumerable<CardTransaction>> OnGetAllCardTransactions(Card existingCard, DateTime? asOfDate = default, int? resultsLimit = default)
        {
            IEnumerable<CardTransaction> transactions = null;

            if (asOfDate.GetValueOrDefault() == default)
                asOfDate = DateTime.Now;

            if (existingCard != null
                && _transactions.TryGetValue(existingCard, out var existingTransactions))
            {
                transactions = (from eachTransaction in existingTransactions
                                where eachTransaction.TransactionDate < asOfDate
                                orderby eachTransaction.TransactionDate descending
                                select eachTransaction);

                if (resultsLimit.HasValue && resultsLimit.Value > 0)
                    transactions = transactions.Take(resultsLimit.Value);

                return transactions;
            }
            else
                transactions = new List<CardTransaction>();

            return await Task.FromResult(transactions);
        }


        async public Task<bool> SaveCard(Card card)
        {
            bool success = false;
            if (card != null)
            {
                var savedCard = _cards.AddOrUpdate(card.Number, card, (cardNumber, existingCard) => card);
                success = savedCard != null;
            }
            return await Task.FromResult(success);
        }

        async public Task<bool> SaveTransaction(CardTransaction transaction)
        {
            bool success = false;

            if (transaction != null)
            {
                var existingCard = await GetCardByNumber(transaction.Card.Number);
                if (existingCard != null)
                {
                    var existingTransactions = _transactions.GetOrAdd(existingCard, new ConcurrentBag<CardTransaction>());
                    existingTransactions.Add(transaction);
                    success = true;
                }
            }
            return await Task.FromResult(success);
        }
    }
}
