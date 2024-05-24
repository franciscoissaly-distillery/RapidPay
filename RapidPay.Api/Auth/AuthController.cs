﻿using Microsoft.AspNetCore.Mvc;
using RapidPay.Domain.Services;

namespace RapidPay.Api.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUsersManager _usersManager;

        public AuthController(IJwtTokenService jwtTokenService, IUsersManager usersManager)
        {
            _jwtTokenService = jwtTokenService;
            _usersManager = usersManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Replace with your user validation logic
            if (_usersManager.IsValidUser(request.Username, request.Password))
            {
                var token = _jwtTokenService.GenerateToken(request.Username);
                return Ok(new { Token = token });
            }

            return Unauthorized();
        }
    }
}
