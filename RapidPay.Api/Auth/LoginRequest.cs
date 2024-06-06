using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RapidPay.Api.Auth
{

    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }
       
        [Required]
        public required string Password { get; set; }
    }
}
