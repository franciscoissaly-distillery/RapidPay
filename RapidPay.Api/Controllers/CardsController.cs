using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RapidPay.Api.Models;
using RapidPay.Domain.Entities;
using RapidPay.Domain.Exceptions;
using RapidPay.Domain.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RapidPay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public partial class CardsController : ControllerBase
    {
        private ICardsManager _cardsManager;
        private ModelsMapper _mapper = new();

        public CardsController(ICardsManager cardsManager) : base()
        {
            ArgumentNullException.ThrowIfNull(cardsManager);
            _cardsManager = cardsManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
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
        public async Task<IActionResult> Post([FromBody] CreateCardRequest request)
        {
            Card newCard = await _cardsManager.CreateCard(request.CardNumber);
            var url = new Uri($"{this.Request.Path.ToUriComponent()}/{newCard.Number}", UriKind.Relative);
            var dto = _mapper.MapToModel(newCard);
            return Created(url, dto);
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
