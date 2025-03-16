using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using questionnaire.Authentication;
using questionnaire.DTOs;
using questionnaire.Models;
using System.Security.Claims;

namespace questionnaire.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly QuestionnaireContext _context;

        public UserController(QuestionnaireContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
    }

    [Route("questionnaire")]
    public class QuestionnaireController : ControllerBase
    {
        private readonly QuestionnaireContext _context;

        public QuestionnaireController(QuestionnaireContext context)
        {
            _context = context;
        }

        // Создание анкеты
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateQuestionnaire([FromBody] CreateQuestionnaire request)
        {
            var userIdClaim = User.FindFirstValue(AuthOptions.UserIdClaimType);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Ошибка получения userId из токена.");
            }

            var questionnaire = new Questionnaire
            {
                Title = request.Title,
                TypeQuestionnaireId = 1, // Всегда создается с типом 1 (open)
                UserId = userId, // Берем из токена
                CreatedAt = DateTime.UtcNow, // Автоматически ставим текущее время
                IsPublished = false // По умолчанию анкета не опубликована
            };

            await _context.Questionnaires.AddAsync(questionnaire); // Используем AddAsync
            await _context.SaveChangesAsync();

            return Ok(new { message = "Анкета успешно создана", questionnaireId = questionnaire.Id });
        }

        [HttpGet("{questionnaireId}")]
        [Authorize]
        public async Task<IActionResult> GetQuestionnaireById(int questionnaireId)
        {
            // Получаем ID текущего пользователя из токена
            var userIdClaim = User.FindFirstValue(AuthOptions.UserIdClaimType);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("Не удалось получить ID пользователя.");
            }

            // Находим анкету с подключенными вопросами, вариантами ответов и их ответами
            var questionnaire = await _context.Questionnaires
                .Include(q => q.Questions.OrderBy(q => q.Id)) // Сортируем вопросы по ID
                    .ThenInclude(q => q.Options.OrderBy(o => o.Order)) // Сортируем варианты ответов по Order
                        .ThenInclude(o => o.Answers) // Подключаем ответы для вариантов ответов
                .Include(q => q.Questions) // Подключаем вопросы снова для загрузки всех ответов
                    .ThenInclude(q => q.Answers) // Подключаем все ответы напрямую
                .FirstOrDefaultAsync(q => q.Id == questionnaireId);

            if (questionnaire == null)
            {
                return NotFound(new { message = "Анкета не найдена." });
            }

            // Проверяем права доступа
            var user = await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            if (questionnaire.UserId != currentUserId && user.AccessLevel.LevelName != "admin")
            {
                return StatusCode(403, new { message = "У вас нет прав для просмотра этой анкеты." });
            }

            // Возвращаем данные анкеты
            return Ok(new
            {
                questionnaire.Id,
                questionnaire.Title,
                questionnaire.CreatedAt,
                questionnaire.IsPublished,
                Questions = questionnaire.Questions.Select(q => new
                {
                    q.Id,
                    q.Text,
                    q.QuestionTypeId,
                    Options = q.Options.Select(o => new
                    {
                        o.Id,
                        o.OptionText,
                        o.Order,
                        Answers = o.Answers.Select(a => new
                        {
                            a.Id,
                            a.Text,
                            a.CreatedAt,
                            a.UserId,
                            a.AnonymousId
                        }).ToList()
                    }).ToList(),
                    Answers = q.Answers.Where(a => a.SelectOption == null).Select(a => new
                    {
                        a.Id,
                        a.Text,
                        a.CreatedAt,
                        a.UserId,
                        a.AnonymousId
                    }).ToList() // Отдельная коллекция для текстовых ответов
                }).ToList()
            });
        }

        [HttpPost("{questionnaireId}/questions/add-question")]
        [Authorize]
        public async Task<IActionResult> AddQuestionWithOptions(int questionnaireId, [FromBody] AddQuestionRequest request)
        {
            // Находим анкету по ID
            var questionnaire = await _context.Questionnaires.FindAsync(questionnaireId);
            if (questionnaire == null)
            {
                return NotFound("Анкета не найдена.");
            }

            // Создаем вопрос
            var question = new Question
            {
                Text = request.Text,
                QuestionnaireId = questionnaireId,
                QuestionTypeId = request.QuestionType
            };

            // Добавляем вопрос в базу данных
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();

            // Если есть варианты ответов, добавляем их
            if (request.Options != null && request.Options.Any())
            {
                // Рассчитываем порядковые номера
                var lastOrder = await _context.Options
                    .Where(o => o.QuestionId == question.Id)
                    .OrderByDescending(o => o.Order)
                    .Select(o => o.Order)
                    .FirstOrDefaultAsync();

                foreach (var optionText in request.Options)
                {
                    var order = lastOrder + 1; // Новый порядковый номер
                    var option = new QuestionOption
                    {
                        QuestionId = question.Id,
                        OptionText = optionText,
                        Order = order // Присваиваем порядковый номер
                    };
                    await _context.Options.AddAsync(option);
                    lastOrder++; // Увеличиваем порядковый номер для следующего варианта
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Вопрос успешно создан.",
                questionId = question.Id
            });
        }

        [HttpPost("questions/{questionId}/answer")]
        public async Task<IActionResult> SubmitAnswer(int questionId, [FromBody] AnswerRequest request)
        {
            // Находим вопрос по ID из пути
            var question = await _context.Questions
                .Include(q => q.Options) // Подключаем варианты ответов
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                return NotFound("Вопрос не найден.");
            }

            // Проверяем, является ли пользователь авторизованным
            var userIdClaim = User.FindFirstValue(AuthOptions.UserIdClaimType);
            int? userId = null;
            int? anonymousId = null;

            if (userIdClaim != null && int.TryParse(userIdClaim, out int parsedUserId))
            {
                userId = parsedUserId; // Авторизованный пользователь
            }
            else
            {
                var sessionIdHeader = Request.Headers["X-Session-Id"].ToString();
                if (!string.IsNullOrEmpty(sessionIdHeader) && Guid.TryParse(sessionIdHeader, out Guid parsedSessionId))
                {
                    var anonymousUser = await _context.Anonymous.FirstOrDefaultAsync(a => a.SessionId == parsedSessionId);
                    if (anonymousUser == null)
                    {
                        return BadRequest("Неверный или потерянный SessionId для анонимного пользователя.");
                    }
                    anonymousId = anonymousUser.Id;
                }
            }

            if (userId == null && anonymousId == null)
            {
                return Unauthorized("Отсутствует проверка подлинности или действительный идентификатор сеанса.");
            }

            // Создаем ответ
            var answer = new Answer
            {
                Text = request.AnswerText,
                QuestionId = questionId,
                UserId = userId,
                AnonymousId = anonymousId,
                CreatedAt = DateTime.UtcNow
            };

            // Обработка выбранных вариантов ответа
            if (request.SelectedOptions != null && request.SelectedOptions.Any())
            {
                foreach (var optionId in request.SelectedOptions)
                {
                    if (!question.Options.Any(o => o.Id == optionId))
                    {
                        return BadRequest($"Неверный вариант ответа: {optionId}");
                    }
                    answer.SelectOption = optionId; // Сохраняем выбранные варианты
                }
            }
            else if (request.SelectedOption.HasValue)
            {
                if (!question.Options.Any(o => o.Id == request.SelectedOption.Value))
                {
                    return BadRequest($"Неверный вариант ответа: {request.SelectedOption.Value}");
                }
                answer.SelectOption = request.SelectedOption.Value; // Сохраняем выбранный вариант
            }

            // Обработка шкального значения
            if (request.ScaleValue.HasValue)
            {
                if (request.ScaleValue < 1 || request.ScaleValue > 10)
                {
                    return BadRequest("Значение шкалы должно быть между 1 и 10.");
                }
                answer.Text = request.ScaleValue.ToString(); // Сохраняем значение шкалы как текст
            }

            // Добавляем ответ в базу данных
            await _context.Answers.AddAsync(answer);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ответ отправлен успешно.",
                answerId = answer.Id,
                questionId = questionId,
                createdAt = answer.CreatedAt
            });
        }
    }
}