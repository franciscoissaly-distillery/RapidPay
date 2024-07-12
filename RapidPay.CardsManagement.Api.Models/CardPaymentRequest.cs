using System.ComponentModel.DataAnnotations;

namespace RapidPay.CardsManagement.Api.Models
{
    public class CardPaymentRequest
    {
        [Required]
        [Range(0, int.MaxValue, MinimumIsExclusive = true)]
        public decimal Amount { get; set; }
    }
}