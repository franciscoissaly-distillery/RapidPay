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
        public IActionResult Get()
        {
            var existingCards = _cardsManager.GetAllExistingCards();
            if (!existingCards.Any())
                return NoContent();

            var dtos = _mapper.MapToModel(existingCards);
            return Ok(dtos);
        }


        [HttpGet("{cardNumber}")]
        public IActionResult GetCardByNumber(string cardNumber)
        {
            var existingCard = _cardsManager.GetCard(cardNumber);
            if (existingCard == null)
                return NotFound();

            var dto = _mapper.MapToModel(existingCard);
            return Ok(dto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] CreateCardRequest request)
        {
            Card newCard = _cardsManager.CreateCard(request.CardNumber);
            var url = new Uri($"{this.Request.Path.ToUriComponent()}/{newCard.Number}", UriKind.Relative);
            var dto = _mapper.MapToModel(newCard);
            return Created(url, dto);
        }

        [HttpGet("{cardNumber}/balance")]
        public IActionResult GetCardBalance(string cardNumber)
        {
            var balance = _cardsManager.GetCardBalance(cardNumber);
            return Ok(balance);
        }

        [HttpGet("{cardNumber}/transactions")]
        public IActionResult GetCardTransactions(string cardNumber)
        {
            var existingTransactions = _cardsManager.GetCardTransactions(cardNumber);
            if (!existingTransactions.Any())
                return NoContent();

            var dtos = _mapper.MapToModel(existingTransactions);
            return Ok(dtos);
        }


        [HttpPost("{cardNumber}/payments")]
        public IActionResult RegisterCardPayment(string cardNumber, [FromBody] CardPaymentRequest request)
        {
            var newPayment = _cardsManager.RegisterCardPayment(cardNumber, request.Amount);
            var dto = _mapper.MapToModel(newPayment);
            return Ok(dto);
        }
    }
}
