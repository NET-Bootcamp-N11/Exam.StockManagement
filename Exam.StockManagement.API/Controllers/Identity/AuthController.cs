﻿using Exam.StockManagement.Application.Abstractions.IServices;
using Exam.StockManagement.Domain;
using Exam.StockManagement.Domain.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Exam.StockManagement.API.Controllers.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Login(RequestLogin model)
        {
            var result = await _authService.GenerateToken(model);

            return Ok(result.Token);
        }

        public async Task<IActionResult> SignUp(RequestSignUp model)
        {
            return Ok(model);
        }
    }
}
