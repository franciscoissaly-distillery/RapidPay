using Microsoft.AspNetCore.Mvc;
using RapidPay.Cards.Api.Models;
using RapidPay.Cards.Domain.Entities;
using RapidPay.Cards.Domain.Services;
using RapidPay.Domain.Entities;
using RapidPay.Framework.Api.Controllers;
using RapidPay.Framework.Api.Mapping;

namespace RapidPay.Cards.Api.Controllers
{

    public partial class CardsController : RapidPayApiControllerBase
    {
        private ICardsManager _cardsManager;
        private IModelMapper _mapper;

        public CardsController(ICardsManager cardsManager, IModelMapper mapper)
        {
            ArgumentNullException.ThrowIfNull(cardsManager);
            _cardsManager = cardsManager;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCards()
        {
            var existingCards = await _cardsManager.GetAllExistingCards();
            if (!existingCards.Any())
                return NoContent();

            var model = _mapper.MapTo<CardDto, Card>(existingCards);
            return Ok(model);
        }


        [HttpGet("{cardNumber}")]
        public async Task<IActionResult> GetCardByNumber(string cardNumber)
        {
            var existingCard = await _cardsManager.GetCard(cardNumber);
            if (existingCard == null)
                return NoContent();

            var model = _mapper.MapTo<CardDto, Card>(existingCard);
            return Ok(model);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
        {
            Card newCard = await _cardsManager.CreateCard(request.CardNumber);
            var model = _mapper.MapTo<CardDto, Card>(newCard);
            return CreatedAtAction(nameof(CreateCard), new { cardNumber = newCard.Number }, model);
        }

        [HttpGet("{cardNumber}/balance")]
        public async Task<IActionResult> GetCardBalance(string cardNumber)
        {
            var balance = await _cardsManager.GetCardBalance(cardNumber);
            return Ok(balance);
        }

        [HttpGet("{cardNumber}/transactions")]
        public async Task<IActionResult> GetCardTransactions(string cardNumber)
        {
            var existingTransactions = await _cardsManager.GetCardTransactions(cardNumber);
            if (!existingTransactions.Any())
                return NoContent();

            var model = _mapper.MapTo<TransactionDto, CardTransaction>(existingTransactions);
            return Ok(model);
        }


        [HttpPost("{cardNumber}/payments")]
        public async Task<IActionResult> RegisterCardPayment(string cardNumber, [FromBody] CardPaymentRequest request)
        {
            var newPayment = await _cardsManager.RegisterCardPayment(cardNumber, request.Amount);
            var model = _mapper.MapTo<TransactionDto, CardTransaction>(newPayment);
            return Ok(model);
        }

        [HttpDelete("{cardNumber}")]
        public async Task<IActionResult> DeleteCard(string cardNumber)
        {
            var didDelete = await _cardsManager.DeleteCard(cardNumber);
            return Ok(didDelete);
        }

    }
}
