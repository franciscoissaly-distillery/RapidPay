using RapidPay.Domain.Entities;
using System.Collections.Concurrent;
using System.Linq;

namespace RapidPay.Domain.Repository
{
    public class CardsManagementInMemoryRepository : ICardsManagementRepository
    {
        private readonly ConcurrentDictionary<string, Card> _cards = new();
        private readonly ConcurrentDictionary<Card, ConcurrentBag<CardTransaction>> _transactions = new();

        public IEnumerable<Card> GetAllCards()
        {
            return _cards.Values;
        }

        public Card GetCardByNumber(string cardNumber)
        {
            if (!string.IsNullOrWhiteSpace(cardNumber)
                && _cards.TryGetValue(cardNumber, out var card))
                return card;
            else
                return null;
        }

        public CardTransaction GetCardLastTransaction(Card existingCard, DateTime? asOfDate = default)
        {
            return OnGetAllCardTransactions(existingCard, asOfDate, 1).FirstOrDefault();
        }

        public IEnumerable<CardTransaction> GetAllCardTransactions(Card existingCard, DateTime? asOfDate = default)
        { 
            return OnGetAllCardTransactions(existingCard,asOfDate);
        }

        private IEnumerable<CardTransaction> OnGetAllCardTransactions(Card existingCard, DateTime? asOfDate = default, int? resultsLimit = default)
        {
            if (asOfDate.GetValueOrDefault() == default)
                asOfDate = DateTime.Now;

            if (existingCard != null
                && _transactions.TryGetValue(existingCard, out var existingTransactions))
            {
                IEnumerable<CardTransaction> transactions = (from eachTransaction in existingTransactions
                                                             where eachTransaction.TransactionDate < asOfDate
                                                             orderby eachTransaction.TransactionDate descending
                                                             select eachTransaction);

                if (resultsLimit.HasValue && resultsLimit.Value > 0)
                    transactions = transactions.Take(resultsLimit.Value);

                return transactions;
            }
            else
                return new List<CardTransaction>();
        }


        public bool SaveCard(Card card)
        {
            if (card == null)
                return false;

            var savedCard = _cards.AddOrUpdate(card.Number, card, (cardNumber, existingCard) => card);
            return savedCard != null;
        }

        public bool SaveTransaction(CardTransaction transaction)
        {
            if (transaction == null)
                return false;

            var existingCard = GetCardByNumber(transaction.Card.Number);
            if (existingCard == null)
                return false;

            var existingTransactions = _transactions.GetOrAdd(existingCard, new ConcurrentBag<CardTransaction>());
            existingTransactions.Add(transaction);
            return true;
        }
    }
}
