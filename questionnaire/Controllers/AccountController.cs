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
using questionnaire.DTOs;
using questionnaire.Authentication;
using questionnaire.Services;

namespace questionnaire.Controllers
{
    [Route("auth")]
    public class AccountController : Controller
    {
        private readonly QuestionnaireContext _context;
        private readonly TokenService _tokenService;

        public AccountController(QuestionnaireContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // Регистрация пользователя
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
            {
                return BadRequest("Username или email уже заняты.");
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = hasher.HashPassword(model, model.PasswordHash),
                AccessLevelId = model.AccessLevelId > 0 ? model.AccessLevelId : 2 // Админ по умолчанию
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                access_token = _tokenService.GenerateAccessToken(user)
            });
        }

        // Логин пользователя
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Login || u.Email == request.Login);
            if (user == null)
            {
                return BadRequest("Неверный username/email или пароль.");
            }

            var hasher = new PasswordHasher<User>();
            var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return BadRequest("Неверный пароль.");
            }

            // Генерация refresh-токена
            var refreshToken = _tokenService.GenerateRefreshToken();

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

            await _context.Tokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                access_token = _tokenService.GenerateAccessToken(user),
                refresh_token = refreshToken
            });
        }

        // Выход пользователя
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Извлекаем идентификатор пользователя из клаймов
            var userIdClaim = User.FindFirst(AuthOptions.UserIdClaimType)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "Неверный пользователь." });
            }
            int userId = int.Parse(userIdClaim);

            // Ищем refresh-токен для текущего пользователя
            var userToken = await _context.Tokens.FirstOrDefaultAsync(t => t.UserId == userId);
            if (userToken == null)
            {
                return BadRequest("Не найдено активных refresh токенов.");
            }

            _context.Tokens.Remove(userToken);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Пользователь успешно вышел." });
        }

        // Создание анонимного пользователя
        [AllowAnonymous]
        [HttpPost("create-anonymous")]
        public async Task<IActionResult> CreateAnonymousUser()
        {
            // Генерация уникального SessionId
            var sessionId = Guid.NewGuid();

            // Проверяем, существует ли уже анонимный пользователь с таким SessionId
            var anonymousUser = await _context.Anonymous.FirstOrDefaultAsync(a => a.SessionId == sessionId);
            if (anonymousUser == null)
            {
                // Создаем нового анонимного пользователя
                anonymousUser = new Anonymou
                {
                    SessionId = sessionId
                };

                await _context.Anonymous.AddAsync(anonymousUser); // Используем AddAsync
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Анонимный пользователь создан.", sessionId = anonymousUser.SessionId });
        }
    }
}
