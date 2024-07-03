using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RapidPay.Api.Framework.Controllers;
using RapidPay.Domain.Adapters;

namespace RapidPay.FeesManagement.Api.Controllers
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
