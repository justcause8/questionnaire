using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using questionnaire.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace questionnaire.Controllers
{
    // Обработка и получение токена для аутентификации пользователей
    [Route("auth")]
    public class AccountController : Controller
    {
        private readonly QuestionnaireContext _context;

        public AccountController(QuestionnaireContext context)
        {
            _context = context;
        }

        // Для регистрации пользователей доступ открыт для анонимов
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
            {
                return BadRequest("Username or email is already taken.");
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = hasher.HashPassword(model, model.PasswordHash),
                AccessLevelId = model.AccessLevelId > 0 ? model.AccessLevelId : 1 // Админ по умолчанию
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                access_token = GenerateAccessToken(user)
            });
        }

        // Логин также доступен для анонимов
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(string login, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == login || u.Email == login);
            if (user == null)
            {
                return BadRequest("Invalid username/email or password.");
            }

            var hasher = new PasswordHasher<User>();
            var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return BadRequest("Invalid password.");
            }

            // Генерация refresh-токена
            var refreshToken = GenerateRefreshToken();

            // Удаляем старый refresh-токен, если он есть
            var existingToken = await _context.Tokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (existingToken != null)
            {
                _context.Tokens.Remove(existingToken);
            }

            // Сохраняем новый refresh-токен в БД
            var userToken = new Token
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                RefreshTokenDatetime = DateTime.UtcNow.AddDays(7) // Refresh-токен на 7 дней
            };

            _context.Tokens.Add(userToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                access_token = GenerateAccessToken(user),
                refresh_token = refreshToken
            });
        }

        // Для выхода требуется авторизация
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Извлекаем идентификатор пользователя из клаймов
            var userIdClaim = User.FindFirst(AuthOptions.UserIdClaimType)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "Invalid user." });
            }
            int userId = int.Parse(userIdClaim);

            // Ищем refresh-токен для текущего пользователя
            var userToken = await _context.Tokens.FirstOrDefaultAsync(t => t.UserId == userId);
            if (userToken == null)
            {
                return BadRequest("No active refresh token found.");
            }

            _context.Tokens.Remove(userToken);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User logged out successfully." });
        }

        private string GenerateAccessToken(User user)
        {
            var identity = GetClaimsIdentity(user);
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthOptions.KEY)),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
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
