using Microsoft.AspNetCore.Mvc;
using RapidPay.Cards.Adapters.Fees;
using RapidPay.Framework.Api.Controllers;

namespace RapidPay.Fees.Api.Controllers
{
    public class PaymentFeesController : RapidPayApiControllerBase
    {
        private readonly IPaymentFeesAdapter _paymentFeesManager;

        public PaymentFeesController(IPaymentFeesAdapter paymentFeesManager)
        {
            _paymentFeesManager = paymentFeesManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetFee([FromQuery] GetFeeRequest request)
        {
            var fee = await _paymentFeesManager.CalculatePaymentFee(request);
            return Ok(fee);
        }
    }
}
