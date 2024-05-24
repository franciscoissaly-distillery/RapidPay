using RapidPay.Domain.Adapters;
using RapidPay.Domain.Entities;
using RapidPay.Domain.Exceptions;
using RapidPay.Domain.Repository;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace RapidPay.Domain.Services
{
    public class CardsManager : ICardsManager
    {
        private readonly ICardsManagementRepository _repository;
        private readonly IPaymentFeesAdapter _paymentFeesManager;


        public CardsManager(ICardsManagementRepository repository, IPaymentFeesAdapter paymentFeesManager)
        {
            ArgumentNullException.ThrowIfNull(repository);
            _repository = repository;

            ArgumentNullException.ThrowIfNull(paymentFeesManager);
            _paymentFeesManager = paymentFeesManager;
        }

        public async Task<decimal> GetCardBalance(string cardNumber, DateTime? asOfDate = default)
        {
            Card existingCard =await GetCard(cardNumber);
            return await OnGetCardBalance(existingCard, asOfDate);
        }

        private async Task<decimal> OnGetCardBalance(Card existingCard, DateTime? asOfDate = default)
        {
            ArgumentNullException.ThrowIfNull(existingCard);

            CardTransaction lastTransaction =await _repository.GetCardLastTransaction(existingCard, asOfDate);
            if (lastTransaction == null)
                return 0;

            return lastTransaction.CardBalanceAmount;
        }


        public async Task<Card> GetCard(string cardNumber)
        {
            Card existingCard =await OnGetCard(cardNumber);
            if (existingCard == null)
                throw new CardsManagementException("Unknown card number")
                {
                    MemberName = nameof(cardNumber),
                    ValueText = cardNumber
                };

            return existingCard;
        }

        private async Task<Card> OnGetCard(string cardNumber)
        {
            if (!IsValidCardNumber(cardNumber))
                throw new CardsManagementException("Invalid card number. Expecting 15 digits")
                {
                    MemberName = nameof(cardNumber),
                    ValueText = cardNumber
                };

            return await _repository.GetCardByNumber(cardNumber);
        }

        public bool IsValidCardNumber(string cardNumber)
        {
            return cardNumber != null
                && cardNumber.Length == 15
                && Regex.IsMatch(cardNumber, @"\d{15}", RegexOptions.Singleline);
        }


        public async Task<Card> CreateCard(string cardNumber)
        {
            Card existingCard = await OnGetCard(cardNumber);
            if (existingCard != null)
                throw new CardsManagementException("Card number already in use")
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
            return await OnRegisterCardTransaction(CardTransactionType.Payment, cardNumber, paymentAmount);
        }


        private async Task<CardTransaction> OnRegisterCardTransaction(CardTransactionType transactionType, string cardNumber, decimal paymentAmount)
        {

            ArgumentNullException.ThrowIfNull(transactionType);

            if (paymentAmount < 0)
                throw new CardsManagementException("Invalid negative payment amount")
                {
                    MemberName = nameof(paymentAmount),
                    ValueText = paymentAmount.ToString()
                };

            Card existingCard = await GetCard(cardNumber);
            var currentBalance = await OnGetCardBalance(existingCard);

            var newPayment = new CardTransaction(existingCard)
            {
                TransactionType = transactionType,
                TransactionDate = DateTime.Now,
                TransactionAmount = paymentAmount,
            };

            if (transactionType.GeneratesFee)
                newPayment.FeeAmount = await _paymentFeesManager.CalculatePaymentFee(newPayment);

            newPayment.CardBalanceAmount = OnCalculateNewBalance(currentBalance, newPayment);

            await _repository.SaveTransaction(newPayment);
            return newPayment;
        }



        private decimal OnCalculateNewBalance(decimal currentBalance, CardTransaction newPayment)
        {
            var newBalance = currentBalance;
            if (newPayment != null)
                newBalance += (newPayment.TransactionAmount + newPayment.FeeAmount) * newPayment.TransactionType.Sign;
            return newBalance;
        }



        public async Task<IEnumerable<Card>> GetAllExistingCards()
        {
            return await _repository.GetAllCards();
        }

        public async Task<IEnumerable<CardTransaction>> GetCardTransactions(string cardNumber)
        {
            Card existingCard = await GetCard(cardNumber);
            return await _repository.GetAllCardTransactions(existingCard);
        }
    }
}
