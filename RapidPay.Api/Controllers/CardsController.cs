using Microsoft.AspNetCore.Mvc;
using RapidPay.Api.Framework.Controllers;
using RapidPay.Api.Models;
using RapidPay.Domain.Entities;
using RapidPay.Domain.Services;

namespace RapidPay.Api.Controllers
{

    public partial class CardsController : RapidPayApiControllerBase
    {
        private ICardsManager _cardsManager;
        private ModelsMapper _mapper = new();

        public CardsController(ICardsManager cardsManager)
        {
            ArgumentNullException.ThrowIfNull(cardsManager);
            _cardsManager = cardsManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCards()
        {
            var existingCards = await _cardsManager.GetAllExistingCards();
            if (!existingCards.Any())
                return NoContent();

            var dtos = _mapper.MapToModel(existingCards);
            return Ok(dtos);
        }


        [HttpGet("{cardNumber}")]
        public async Task<IActionResult> GetCardByNumber(string cardNumber)
        {
            var existingCard = await _cardsManager.GetCard(cardNumber);
            if (existingCard == null)
                return NotFound();

            var dto = _mapper.MapToModel(existingCard);
            return Ok(dto);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
        {
            Card newCard = await _cardsManager.CreateCard(request.CardNumber);
            var dto = _mapper.MapToModel(newCard);
            return CreatedAtAction(nameof(CreateCard), new { cardNumber = newCard.Number }, dto);
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

            var dtos = _mapper.MapToModel(existingTransactions);
            return Ok(dtos);
        }


        [HttpPost("{cardNumber}/payments")]
        public async Task<IActionResult> RegisterCardPayment(string cardNumber, [FromBody] CardPaymentRequest request)
        {
            var newPayment = await _cardsManager.RegisterCardPayment(cardNumber, request.Amount);
            var dto = _mapper.MapToModel(newPayment);
            return Ok(dto);
        }
    }
}
