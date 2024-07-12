using Microsoft.AspNetCore.Mvc;
using RapidPay.Auth.Api.Logic;
using RapidPay.Auth.Api.Models;
using RapidPay.Cards.Adapters;

namespace RapidPay.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUsersAdapter _usersManager;

        public AuthController(IJwtTokenService jwtTokenService, IUsersAdapter usersManager)
        {
            _jwtTokenService = jwtTokenService;
            _usersManager = usersManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (_usersManager.IsValidUser(request.Username, request.Password))
            {
                var token = _jwtTokenService.GenerateToken(request.Username);
                return Ok(new LoginResponse { Token = token });
            }

            return Unauthorized();
        }
    }
}
