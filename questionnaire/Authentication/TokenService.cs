using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using questionnaire.Authentication;
using questionnaire.Models;

namespace questionnaire.Services
{
    public class TokenService
    {
        private readonly string _jwtKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _lifetimeMinutes;

        public TokenService(string jwtKey, string issuer, string audience, int lifetimeMinutes)
        {
            _jwtKey = jwtKey;
            _issuer = issuer;
            _audience = audience;
            _lifetimeMinutes = lifetimeMinutes;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = GetClaimsIdentity(user).Claims;

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                notBefore: now,
                claims: claims,
                expires: now.Add(TimeSpan.FromMinutes(_lifetimeMinutes)),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsIdentity GetClaimsIdentity(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username ?? user.Email),
                new Claim(AuthOptions.UserIdClaimType, user.Id.ToString())
            };

            return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
    }
}