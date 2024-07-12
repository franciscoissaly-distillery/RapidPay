using RapidPay.Cards.Api.Models;
using RapidPay.Cards.Domain.Entities;
using RapidPay.Domain.Entities;

namespace RapidPay.Cards.Api.Controllers
{
    public class ModelsMapper
    {
        public IEnumerable<CardDto> MapToModel(IEnumerable<Card> cards)
        {
            ArgumentNullException.ThrowIfNull(cards);
            return from card in cards
                   select MapToModel(card);
        }

        public CardDto MapToModel(Card card)
        {
            ArgumentNullException.ThrowIfNull(card);
            return new CardDto() { Number = card.Number };
        }
        public IEnumerable<TransactionDto> MapToModel(IEnumerable<CardTransaction> transactions)
        {
            ArgumentNullException.ThrowIfNull(transactions);
            return from transaction in transactions
                   select MapToModel(transaction);
        }

        public TransactionDto MapToModel(CardTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            return new TransactionDto()
            {
                CardNumber = transaction.Card.Number,
                TypeCode = transaction.TransactionType.SystemCode,
                TypeName = transaction.TransactionType.Name,
                Date = transaction.TransactionDate,
                Amount = transaction.TransactionAmount,
                FeeAmount = transaction.FeeAmount,
                BalanceAmount = transaction.CardBalanceAmount
            };
        }
    }
}
