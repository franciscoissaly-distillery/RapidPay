using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RapidPay.Api.Models
{
    public class CreateCardRequest
    {
        [Required]
        [MinLength(15), MaxLength(15)]
        public required string CardNumber { get; set; }
    }
}
