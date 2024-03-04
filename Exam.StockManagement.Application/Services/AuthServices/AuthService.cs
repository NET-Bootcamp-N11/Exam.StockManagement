﻿using Exam.StockManagement.Application.Abstractions.IServices;
using Exam.StockManagement.Domain.Entities.DTOs.Auth;
using Exam.StockManagement.Domain.Entities.Models;
using Exam.StockManagement.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Exam.StockManagement.Application.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _conf;
        private readonly IUserService _userService;

        public AuthService(IConfiguration conf, IUserService userService)
        {
            _conf = conf;
            _userService = userService;
        }

        public async Task<string> CorrectEmail(RegisterLogin user)
        {
            var result = await _userService.GetByEmail(user.Email);
            if (result.Code == user.Code)
            {
                return "Login successfully!";
            }
            throw new NotFoundException();
        }

        public async Task<ResponseLogin> GenerateToken(CheckEmail user, string path)
        {
            var login = new RequestLogin()
            {
                Email = user.Email,
            };

            if (File.ReadAllText(path) != user.Code && await UserExist(login))
            {
                throw new PasswordNotMatchException();
            }

            File.WriteAllText(path, "");

            if (user is null)
            {
                throw new NotFoundException();
            }

            var result = await _userService.GetByEmail(user.Email);

            IEnumerable<int> permissionsId = new List<int>();
            if (result.Role == "Admin")
                permissionsId = new List<int>() { 101, 102, 103, 104, 105, 106, 107, 108, 201, 202, 203, 204, 205, 206, 207, 208 };
            else if (result.Role == "Client")
                permissionsId = new List<int>() { 201, 202, 203, 204, 205, 206, 207, 208 };

            string permmisionJson = JsonSerializer.Serialize(permissionsId);


            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Role, result.Role),
                new Claim("Login", user.Email),
                new Claim("UserID", result.Id.ToString()),
                new Claim("CreatedDate", DateTime.UtcNow.ToString()),
                new Claim("permissions",permmisionJson)
            };

            return await GenerateToken(claims);
        }

        public async Task<ResponseLogin> GenerateToken(IEnumerable<Claim> additionalClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["JWT:Secret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var exDate = Convert.ToInt32(_conf["JWT:ExpireDate"] ?? "10");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64)
            };

            if (additionalClaims?.Any() == true)
                claims.AddRange(additionalClaims);


            var token = new JwtSecurityToken(
                issuer: _conf["JWT:ValidIssuer"],
                audience: _conf["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(exDate),
                signingCredentials: credentials);

            return new ResponseLogin()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };


        }

        public async Task<bool> UserExist(RequestLogin user)
        {
            if (user.Email == null)
            {
                throw new NotFoundException();
            }

            var result = await _userService.GetByEmail(user.Email);

            if (result != null)
            {
                return true;
            }
            return false;
        }

        public async Task<User> RegisterUser(RequestSignUp signUp)
        {
            var result = await _userService.Create(signUp);
            return result;
        }
    }
}
