using RapidPay.Domain.Entities;
using RapidPay.Domain.Exceptions;
using RapidPay.Domain.Repository;
using System.Text.RegularExpressions;

namespace RapidPay.Domain.Services
{
    public class CardsManager : ICardsManager
    {
        private readonly ICardsManagementRepository _repository;
        private readonly IPaymentFeesManager _paymentFeesManager;


        public CardsManager(ICardsManagementRepository repository, IPaymentFeesManager paymentFeesManager)
        {
            ArgumentNullException.ThrowIfNull(repository);
            _repository = repository;

            ArgumentNullException.ThrowIfNull(paymentFeesManager);
            _paymentFeesManager = paymentFeesManager;
        }

        public decimal GetCardBalance(string cardNumber, DateTime? asOfDate = default)
        {
            Card existingCard = GetCard(cardNumber);
            return OnGetCardBalance(existingCard, asOfDate);
        }

        private decimal OnGetCardBalance(Card existingCard, DateTime? asOfDate = default)
        {
            ArgumentNullException.ThrowIfNull(existingCard);

            CardTransaction lastTransaction = _repository.GetCardLastTransaction(existingCard, asOfDate);
            if (lastTransaction == null)
                return 0;

            return lastTransaction.CardBalanceAmount;
        }


        public Card GetCard(string cardNumber)
        {
            Card existingCard = OnGetCard(cardNumber);
            if (existingCard == null)
                throw new CardsManagementException("Unknown card number")
                {
                    MemberName = nameof(cardNumber),
                    ValueText = cardNumber
                };

            return existingCard;
        }

        private Card OnGetCard(string cardNumber)
        {
            if (!IsValidCardNumber(cardNumber))
                throw new CardsManagementException("Invalid card number. Expecting 15 digits")
                {
                    MemberName = nameof(cardNumber),
                    ValueText = cardNumber
                };

            return _repository.GetCardByNumber(cardNumber);
        }

        public bool IsValidCardNumber(string cardNumber)
        {
            return cardNumber != null
                && cardNumber.Length == 15
                && Regex.IsMatch(cardNumber, @"\d{15}", RegexOptions.Singleline);
        }


        public Card CreateCard(string cardNumber)
        {
            Card existingCard = OnGetCard(cardNumber);
            if (existingCard != null)
                throw new CardsManagementException("Card number already in use")
                {
                    MemberName = nameof(cardNumber),
                    ValueText = cardNumber
                };

            var newCard = new Card(cardNumber);
            _repository.SaveCard(newCard);
            return newCard;
        }

        public CardTransaction RegisterCardPayment(string cardNumber, decimal paymentAmount)
        {
            return OnRegisterCardTransaction(CardTransactionType.Payment, cardNumber, paymentAmount);
        }


        private CardTransaction OnRegisterCardTransaction(CardTransactionType transactionType, string cardNumber, decimal paymentAmount)
        {

            ArgumentNullException.ThrowIfNull(transactionType);

            if (paymentAmount < 0)
                throw new CardsManagementException("Invalid negative payment amount")
                {
                    MemberName = nameof(paymentAmount),
                    ValueText = paymentAmount.ToString()
                };

            Card existingCard = GetCard(cardNumber);
            var currentBalance = OnGetCardBalance(existingCard);

            var newPayment = new CardTransaction(existingCard)
            {
                TransactionType = transactionType,
                TransactionDate = DateTime.Now,
                TransactionAmount = paymentAmount,
            };

            if (transactionType.GeneratesFee)
                newPayment.FeeAmount = _paymentFeesManager.CalculatePaymentFee(newPayment);

            newPayment.CardBalanceAmount = OnCalculateNewBalance(currentBalance, newPayment);

            _repository.SaveTransaction(newPayment);
            return newPayment;
        }



        private decimal OnCalculateNewBalance(decimal currentBalance, CardTransaction newPayment)
        {
            var newBalance = currentBalance;
            if (newPayment != null)
                newBalance += (newPayment.TransactionAmount + newPayment.FeeAmount) * newPayment.TransactionType.Sign;
            return newBalance;
        }



        public IEnumerable<Card> GetAllExistingCards()
        {
            return _repository.GetAllCards();
        }

        public IEnumerable<CardTransaction> GetCardTransactions(string cardNumber)
        {
            Card existingCard = GetCard(cardNumber);
            return _repository.GetAllCardTransactions(existingCard);
        }
    }
}
