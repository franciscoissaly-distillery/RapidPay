using RapidPay.Cards.Adapters;
using RapidPay.Cards.Domain.Entities;
using RapidPay.Cards.Domain.Repository;
using RapidPay.Domain.Entities;
using RapidPay.Framework.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace RapidPay.Cards.Domain.Services
{
    public class CardsManager : ICardsManager
    {
        private readonly ICardsRepository _repository;
        private readonly IPaymentFeesAdapter _paymentFeesManager;

        public CardsManager(ICardsRepository repository, IPaymentFeesAdapter paymentFeesManager)
        {
            ArgumentNullException.ThrowIfNull(repository);
            _repository = repository;

            ArgumentNullException.ThrowIfNull(paymentFeesManager);
            _paymentFeesManager = paymentFeesManager;
        }

        public async Task<decimal> GetCardBalance(string cardNumber, DateTime? asOfDate = default)
        {
            Card? existingCard = await GetExistingCard(cardNumber);
            return await OnGetCardBalance(existingCard, asOfDate);
        }

        private async Task<decimal> OnGetCardBalance(Card existingCard, DateTime? asOfDate = default)
        {
            ArgumentNullException.ThrowIfNull(existingCard);
            return await _repository.GetBalanceAmountFromLastTransaction(existingCard, asOfDate);
        }

        public async Task<Card?> GetCard(string cardNumber)
        {
            Card? existingCard = await OnGetCard(cardNumber);
            return existingCard;
        }

        public async Task<Card> GetExistingCard(string cardNumber)
        {
            Card? existingCard = await OnGetCard(cardNumber);
            if (existingCard == null)
                throw new DomainValidationException("Unknown card number")
                {
                    MemberName = nameof(cardNumber),
                    ValueText = cardNumber,
                    InvalidCategory = InvalidCategoryEnum.UnknownEntity
                };

            return existingCard;
        }

        private async Task<Card?> OnGetCard(string cardNumber)
        {
            if (!IsValidCardNumber(cardNumber))
                throw new DomainValidationException("Invalid card number. Expecting 15 digits")
                {
                    MemberName = nameof(cardNumber),
                    ValueText = cardNumber
                };

            return await _repository.GetCard(cardNumber);
        }

        public bool IsValidCardNumber(string cardNumber)
        {
            return cardNumber != null
                && cardNumber.Length == 15
                && Regex.IsMatch(cardNumber, @"\d{15}", RegexOptions.Singleline);
        }

        public async Task<Card> CreateCard(string cardNumber)
        {
            Card? existingCard = await OnGetCard(cardNumber);
            if (existingCard != null)
                throw new DomainValidationException("Card number already in use")
                {
                    MemberName = nameof(cardNumber),
                    ValueText = cardNumber
                };

            var newCard = new Card(cardNumber);
            await _repository.SaveCard(newCard);
            return newCard;
        }

        public async Task<CardTransaction> RegisterCardPayment(string cardNumber, decimal paymentAmount)
        {
            var transactionType = await _repository.GetTransactionType(CardTransactionTypeEnum.Payment.ToString());
            return await OnRegisterCardTransaction(transactionType, cardNumber, paymentAmount);
        }

        private async Task<CardTransaction> OnRegisterCardTransaction(CardTransactionType? transactionType, string cardNumber, decimal paymentAmount)
        {

            ArgumentNullException.ThrowIfNull(transactionType);

            if (paymentAmount < 0)
                throw new DomainValidationException("Invalid negative payment amount")
                {
                    MemberName = nameof(paymentAmount),
                    ValueText = paymentAmount.ToString()
                };

            Card existingCard = await GetExistingCard(cardNumber);
            var currentBalance = await OnGetCardBalance(existingCard);

            var newPayment = new CardTransaction(existingCard)
            {
                TransactionType = transactionType,
                TransactionDate = DateTime.UtcNow,
                TransactionAmount = paymentAmount,
            };

            if (transactionType.GeneratesFee)
                newPayment.FeeAmount = await _paymentFeesManager.CalculatePaymentFee(new GetFeeRequest
                {
                    TransactionDate = newPayment.TransactionDate,
                    TransactionAmount = newPayment.TransactionAmount,
                });

            newPayment.CardBalanceAmount = OnCalculateNewBalance(currentBalance, newPayment);

            await _repository.SaveTransaction(newPayment);
            return newPayment;
        }

        private decimal OnCalculateNewBalance(decimal currentBalance, CardTransaction newPayment)
        {
            var newBalance = currentBalance;
            if (newPayment != null)
                newBalance += Math.Round((newPayment.TransactionAmount + newPayment.FeeAmount) * newPayment.TransactionType.Sign, 2);
            return newBalance;
        }

        public async Task<IEnumerable<Card>> GetAllExistingCards()
        {
            return await _repository.GetAllCards();
        }

        public async Task<IEnumerable<CardTransaction>> GetCardTransactions(string cardNumber)
        {
            Card existingCard = await GetExistingCard(cardNumber);
            return await _repository.GetAllCardTransactions(existingCard);
        }

        public async Task<bool> DeleteCard(string cardNumber)
        {
            Card existingCard = await GetExistingCard(cardNumber);
            return await _repository.DeleteCard(existingCard);
        }
    }
}
