using System.ComponentModel.DataAnnotations;

namespace RapidPay.Cards.Api.Models
{
    public class CreateCardRequest
    {
        [Required]
        [MinLength(15), MaxLength(15)]
        public required string CardNumber { get; set; }
    }
}
