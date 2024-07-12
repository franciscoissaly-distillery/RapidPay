using Microsoft.Extensions.Configuration;
using RapidPay.Cards.Api.Models;
using RapidPay.Framework.Api.ApiClient;
using RapidPay.Framework.Api.Authentication;

namespace RapidPay.Api.Test.Clients
{
    public class CardsApiClient : WebApiClientBase
    {
        private const string CardsUrl = "Cards";
        private const string CardByKeyTemplate = CardsUrl + "/{cardNumber}";


        public CardsApiClient(
                IHttpClientFactory clientFactory,
                IAuthTokenProvider tokenProvider,
                IConfiguration configuration)
                : base(clientFactory, tokenProvider, configuration)
        { }

        public async Task<List<CardDto>?> GetAllCards()
        {
            return await InvokeGet<List<CardDto>?>(CardsUrl);
        }

        public async Task<CardDto?> GetCardByNumber(string cardNumber)
        {
            return await InvokeGetByKey<CardDto?, string>(CardsUrl, cardNumber);
        }

        public async Task<CardDto> CreateCard(CreateCardRequest request)
        {
            return await InvokePost<CardDto, CreateCardRequest>(CardsUrl, request);
        }

        public async Task<decimal> GetCardBalance(string cardNumber)
        {
            return await InvokeGetByKey<decimal>(CardByKeyTemplate + "/balance", new { cardNumber });
        }

        public async Task<List<TransactionDto>?> GetCardTransactions(string cardNumber)
        {
            return await InvokeGetByKey<List<TransactionDto>>(CardByKeyTemplate + "/transactions", new { cardNumber });
        }

        public async Task<TransactionDto> RegisterCardPayment(string cardNumber, CardPaymentRequest request)
        {
            return await InvokePostByKey<TransactionDto, CardPaymentRequest>(CardByKeyTemplate + "/payments", new { cardNumber }, request);
        }

        internal async Task<bool> DeleteCard(string cardNumber)
        {
            return await InvokeDeleteByKey<bool, string>(CardsUrl, cardNumber);
        }
    }
}