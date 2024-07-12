using Microsoft.Extensions.DependencyInjection;
using RapidPay.Api.Test.Clients;
using RapidPay.Cards.Api.Models;

namespace RapidPay.Api.Test.Base
{
    public class CardsApiTestsBase : ApiTestFixtureBase
    {
        protected override void OnAddServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient<CardsApiClient>();
            serviceCollection.AddTransient<CardsApiClient>();
        }

        protected async Task<CardsApiClient> InitCardsApiClient()
        {
            var apiClient = GetService<CardsApiClient>();
            Assert.That(apiClient, Is.Not.Null, "Cards API client");

            bool loginSuccess = await LoginTestUser();
            Assert.IsTrue(loginSuccess, "Login success");

            return apiClient;
        }

        protected async Task<CardDto> InitTestCard()
        {
            if (TestCard is not null)
                return TestCard;

            var testCardNumber = new string('0', 15);

            await CheckNonExistingCard(testCardNumber);
            TestCard = await CreateCard(testCardNumber);
            return TestCard;
        }

        private CardDto? TestCard { get; set; }

        protected override async Task OnFixtureCleanUp()
        {
            if (TestCard is not null)
            {
                await DeleteCard(TestCard.Number);
                TestCard = null;
            }
        }

        protected async Task CheckNonExistingCard(string cardNumber)
        {
            var existingTestCard = await GetCard(cardNumber);
            Assert.That(existingTestCard, Is.Null, $"Existing card '{cardNumber}'");
        }

        protected async Task<CardDto?> GetCard(string cardNumber)
        {
            var apiClient = await InitCardsApiClient();
            var existingTestCard = await apiClient.GetCardByNumber(cardNumber);
            return existingTestCard;
        }

        protected async Task<CardDto> CreateCard(string cardNumber)
        {
            var apiClient = await InitCardsApiClient();
            var newTestCard = await apiClient.CreateCard(new CreateCardRequest { CardNumber = cardNumber });

            var Card = "Created card: ";
            Assert.That(newTestCard, Is.Not.Null, Card);
            Assert.That(newTestCard.Number, Is.EqualTo(cardNumber), Card + nameof(newTestCard.Number));
            return newTestCard;
        }

        protected async Task DeleteCard(string cardNumber)
        {
            var apiClient = await InitCardsApiClient();
            var didDelete = await apiClient.DeleteCard(cardNumber);
            Assert.That(didDelete, Is.True, "Card is deleted");

            await CheckNonExistingCard(cardNumber);
        }


        protected async Task<List<TransactionDto>?> GetCardTransactions(string cardNumber)
        {
            var apiClient = await InitCardsApiClient();
            var transactions = await apiClient.GetCardTransactions(cardNumber);
            return transactions;
        }

        protected async Task<decimal> GetCardBalance(string cardNumber)
        {
            var apiClient = await InitCardsApiClient();
            var balance = await apiClient.GetCardBalance(cardNumber);
            return balance;
        }

        protected async Task<TransactionDto> PostNewPayment(string cardNumber, decimal amount)
        {
            var apiClient = await InitCardsApiClient();
            var newPayment = await apiClient.RegisterCardPayment(cardNumber, new CardPaymentRequest { Amount = amount });

            const string Transaction = "Created transaction: ";
            Assert.That(newPayment, Is.Not.Null, Transaction);
            Assert.That(newPayment.CardNumber, Is.EqualTo(cardNumber), Transaction + nameof(newPayment.CardNumber));
            Assert.That(newPayment.Amount, Is.EqualTo(amount), Transaction + nameof(newPayment.Amount));
            return newPayment;
        }
    }
}