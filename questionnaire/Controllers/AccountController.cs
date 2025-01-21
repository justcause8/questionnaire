using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using questionnaire.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace questionnaire.Controllers
{
    //Обработка и получение токена для аутентификации пользователей
    [Route("auth")]
    public class AccountController : Controller
    {
        private readonly questionnaireContext _context;

        public AccountController(questionnaireContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            // Проверка, существует ли уже пользователь с таким именем или email
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

            if (existingUser != null)
            {
                return BadRequest("Username or email is already taken.");
            }

            // Создание нового пользователя
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = model.PasswordHash // Пока пароль хранится в открытом виде
            };

            // Добавление пользователя в базу данных
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully" });
        }


        [HttpPost("login")]
        public IActionResult Token(string login, string password) //Создание токена
        {
            var identity = GetIdentity(login, password);
            if (identity == null)
            {
                return BadRequest("Invalid username/email or password.");
            }

            var now = DateTime.UtcNow;
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthOptions.KEY));

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                login = identity.Name
            };

            return Json(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { Message = "User logged out successfully" });
        }

        private ClaimsIdentity GetIdentity(string login, string password) // Поиск пользователя по username или email
        {
            var user = _context.Users.SingleOrDefault(u => (u.Username == login || u.Email == login) && u.PasswordHash == password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username ?? user.Email), // Если нет username, используем email
                    new Claim(AuthOptions.UserIdClaimType, user.Id.ToString()) // Добавляем ID пользователя в токен
                };

                return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            }

            return null;
        }
    }
}
