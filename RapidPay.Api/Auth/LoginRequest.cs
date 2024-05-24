using System.Threading.Tasks;

namespace RapidPay.Api.Auth
{

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
