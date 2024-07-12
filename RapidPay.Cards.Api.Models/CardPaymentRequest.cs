using System.ComponentModel.DataAnnotations;

namespace RapidPay.Cards.Api.Models
{
    public class CardPaymentRequest
    {
        [Required]
        [Range(0, int.MaxValue, MinimumIsExclusive = true)]
        public decimal Amount { get; set; }
    }
}