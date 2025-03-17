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

        private async Task<(int? userId, int? anonymousId)> GetUserIdAndAnonymousIdAsync()
        {
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
                        throw new UnauthorizedAccessException("Неверный или потерянный SessionId для анонимного пользователя.");
                    }
                    anonymousId = anonymousUser.Id;
                }
            }

            if (userId == null && anonymousId == null)
            {
                throw new UnauthorizedAccessException("Отсутствует проверка подлинности или действительный идентификатор сеанса.");
            }

            return (userId, anonymousId);
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

            // Проверяем права доступа
            var (userId, _) = await GetUserIdAndAnonymousIdAsync();

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

                // Если lastOrder == 0, начнем нумерацию с 1
                int startOrder = lastOrder == 0 ? 1 : lastOrder + 1;

                foreach (var optionText in request.Options)
                {
                    var option = new QuestionOption
                    {
                        QuestionId = question.Id,
                        OptionText = optionText,
                        Order = startOrder // Присваиваем порядковый номер
                    };
                    await _context.Options.AddAsync(option);
                    startOrder++; // Увеличиваем порядковый номер для следующего варианта
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Вопрос успешно создан.",
                questionId = question.Id
            });
        }

        [HttpPost("{questionnaireId}/questions/{questionId}/answer")]
        public async Task<IActionResult> SubmitAnswer(int questionnaireId, int questionId, [FromBody] AnswerRequest request)
        {
            // Проверка на null
            if (request == null)
            {
                return BadRequest("Тело запроса не может быть пустым.");
            }

            // Находим анкету по ID
            var questionnaire = await _context.Questionnaires.FindAsync(questionnaireId);
            if (questionnaire == null)
            {
                return NotFound("Анкета не найдена.");
            }

            // Находим вопрос по ID из пути
            var question = await _context.Questions
                .Include(q => q.Options) // Подключаем варианты ответов
                .FirstOrDefaultAsync(q => q.Id == questionId && q.QuestionnaireId == questionnaireId);

            if (question == null)
            {
                return NotFound("Вопрос не найден в указанной анкете.");
            }

            // Проверяем права доступа
            var (userId, anonymousId) = await GetUserIdAndAnonymousIdAsync();

            // Обработка ответа в зависимости от типа вопроса
            switch (question.QuestionTypeId)
            {
                case 1: // Текстовый вопрос
                    if (string.IsNullOrEmpty(request.AnswerText))
                    {
                        return BadRequest("Для текстового вопроса требуется поле 'AnswerText'.");
                    }

                    var textAnswer = new Answer
                    {
                        Text = request.AnswerText,
                        QuestionId = questionId,
                        UserId = userId,
                        AnonymousId = anonymousId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Answers.AddAsync(textAnswer);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Текстовый ответ успешно отправлен.",
                        answerId = textAnswer.Id
                    });

                case 2: // Выбор одного варианта
                    if (!request.AnswerClose.HasValue)
                    {
                        return BadRequest("Для выбора одного варианта требуется поле 'AnswerClose'.");
                    }

                    var singleOption = question.Options.FirstOrDefault(o => o.Order == request.AnswerClose.Value);
                    if (singleOption == null)
                    {
                        return BadRequest($"Неверный вариант ответа: {request.AnswerClose.Value}");
                    }

                    var singleAnswer = new Answer
                    {
                        Text = null,
                        QuestionId = questionId,
                        UserId = userId,
                        AnonymousId = anonymousId,
                        CreatedAt = DateTime.UtcNow,
                        SelectOption = singleOption.Id
                    };

                    await _context.Answers.AddAsync(singleAnswer);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Ответ успешно отправлен.",
                        answerId = singleAnswer.Id
                    });

                case 3: // Выбор нескольких вариантов
                    if (request.AnswerMultiple == null || !request.AnswerMultiple.Any())
                    {
                        return BadRequest("Для выбора нескольких вариантов требуется поле 'AnswerMultiple'.");
                    }

                    foreach (var order in request.AnswerMultiple)
                    {
                        var option = question.Options.FirstOrDefault(o => o.Order == order);
                        if (option == null)
                        {
                            return BadRequest($"Неверный вариант ответа: {order}");
                        }

                        var multipleAnswer = new Answer
                        {
                            Text = null,
                            QuestionId = questionId,
                            UserId = userId,
                            AnonymousId = anonymousId,
                            CreatedAt = DateTime.UtcNow,
                            SelectOption = option.Id
                        };

                        await _context.Answers.AddAsync(multipleAnswer);
                    }
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Ответы успешно отправлены."
                    });

                case 4: // Шкальный вопрос
                    if (!request.AnswerScale.HasValue)
                    {
                        return BadRequest("Для шкального вопроса требуется поле 'AnswerScale'.");
                    }

                    if (request.AnswerScale < 1 || request.AnswerScale > 10)
                    {
                        return BadRequest("Значение шкалы должно быть между 1 и 10.");
                    }

                    var scaleAnswer = new Answer
                    {
                        Text = request.AnswerScale.ToString(),
                        QuestionId = questionId,
                        UserId = userId,
                        AnonymousId = anonymousId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Answers.AddAsync(scaleAnswer);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Шкальный ответ успешно отправлен.",
                        answerId = scaleAnswer.Id
                    });

                default:
                    return BadRequest("Неизвестный тип вопроса.");
            }
        }

        // Вывод полной информации по анкете
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
                    }).ToList()
                }).ToList()
            });
        }

        // Метод для отображения списка анкет
        [HttpGet("user/questionnaires")]
        [Authorize]
        public async Task<IActionResult> GetUserQuestionnaires()
        {
            var userIdClaim = User.FindFirstValue(AuthOptions.UserIdClaimType);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("Не удалось получить ID пользователя.");
            }

            var questionnaires = await _context.Questionnaires
                .Where(q => q.UserId == currentUserId)
                .Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.CreatedAt,
                    q.IsPublished
                })
                .ToListAsync();

            return Ok(new
            {
                questionnaires
            });
        }

        // Метод для редактирования названия анкеты
        [HttpPut("{questionnaireId}/title")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestionnaireTitle(int questionnaireId, [FromBody] UpdateQuestionnaireTitleRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.NewTitle))
            {
                return BadRequest("Поле 'NewTitle' обязательно.");
            }

            var (userId, _) = await GetUserIdAndAnonymousIdAsync();

            var questionnaire = await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == questionnaireId && q.UserId == userId);

            if (questionnaire == null)
            {
                return NotFound("Анкета не найдена.");
            }

            questionnaire.Title = request.NewTitle;
            _context.Questionnaires.Update(questionnaire);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Название анкеты успешно обновлено.",
                title = questionnaire.Title
            });
        }

        // Метод для удаления анкеты
        [HttpDelete("{questionnaireId}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestionnaire(int questionnaireId)
        {
            // Проверяем права доступа
            var (userId, _) = await GetUserIdAndAnonymousIdAsync();

            // Находим анкету по ID
            var questionnaire = await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == questionnaireId && q.UserId == userId);

            if (questionnaire == null)
            {
                return NotFound("Анкета не найдена.");
            }

            // Удаляем анкету
            _context.Questionnaires.Remove(questionnaire);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Анкета успешно удалена."
            });
        }

        // Метод для изменения статуса анкеты
        [HttpPut("{questionnaireId}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestionnaireStatus(int questionnaireId, [FromBody] UpdateQuestionnaireStatusRequest request)
        {
            var userIdClaim = User.FindFirstValue(AuthOptions.UserIdClaimType);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("Не удалось получить ID пользователя.");
            }

            var questionnaire = await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == questionnaireId && q.UserId == currentUserId);

            if (questionnaire == null)
            {
                return NotFound("Анкета не найдена.");
            }

            questionnaire.IsPublished = request.IsPublished;
            _context.Questionnaires.Update(questionnaire);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Статус анкеты успешно обновлен.",
                isPublished = questionnaire.IsPublished
            });
        }

        // Метод для редактирования текста вопроса
        [HttpPut("{questionnaireId}/questions/{questionId}/text")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestionText(int questionnaireId, int questionId, [FromBody] UpdateQuestionTextRequest request)
        {
            // Проверяем права доступа
            var (userId, _) = await GetUserIdAndAnonymousIdAsync();

            // Находим вопрос по ID и ID анкеты
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == questionId && q.QuestionnaireId == questionnaireId && q.Questionnaire.UserId == userId);

            if (question == null)
            {
                return NotFound("Вопрос не найден или не принадлежит указанной анкете.");
            }

            // Обновляем текст вопроса
            question.Text = request.NewText;
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Текст вопроса успешно обновлен.",
                newText = question.Text
            });
        }

        // Метод по изменению типа вопроса
        [HttpPut("{questionnaireId}/questions/{questionId}/type")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestionType(int questionnaireId, int questionId, [FromBody] UpdateQuestionTypeRequest request)
        {
            // Проверяем права доступа
            var (userId, _) = await GetUserIdAndAnonymousIdAsync();

            // Находим вопрос по ID и ID анкеты
            var question = await _context.Questions
                .Include(q => q.Options) // Подключаем варианты ответов для удаления
                .FirstOrDefaultAsync(q => q.Id == questionId && q.QuestionnaireId == questionnaireId && q.Questionnaire.UserId == userId);

            if (question == null)
            {
                return NotFound("Вопрос не найден или не принадлежит указанной анкете.");
            }

            // Проверяем корректность нового типа вопроса
            if (request.NewQuestionType < 1 || request.NewQuestionType > 4)
            {
                return BadRequest("Неверный тип вопроса. Допустимые значения: 1 (текстовый), 2 (выбор одного варианта), 3 (множественный выбор), 4 (шкальный).");
            }

            // Удаляем варианты ответов, если новый тип вопроса не поддерживает их
            if (request.NewQuestionType == 1 || request.NewQuestionType == 4) // Текстовый или шкальный вопрос
            {
                var optionsToRemove = await _context.Options
                    .Where(o => o.QuestionId == questionId)
                    .ToListAsync();

                if (optionsToRemove.Any())
                {
                    _context.Options.RemoveRange(optionsToRemove); // Удаляем все варианты ответов
                }
            }

            // Обновляем тип вопроса
            question.QuestionTypeId = request.NewQuestionType;
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Тип вопроса успешно обновлен.",
                newQuestionType = question.QuestionTypeId
            });
        }

        // Метод для добавления варианта ответа
        [HttpPost("{questionnaireId}/questions/{questionId}/options")]
        [Authorize]
        public async Task<IActionResult> AddQuestionOption(int questionnaireId, int questionId, [FromBody] AddQuestionOptionRequest request)
        {
            // Проверяем права доступа
            var (userId, _) = await GetUserIdAndAnonymousIdAsync();

            // Находим вопрос по ID и ID анкеты
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == questionId && q.QuestionnaireId == questionnaireId && q.Questionnaire.UserId == userId);

            if (question == null)
            {
                return NotFound("Вопрос не найден или не принадлежит указанной анкете.");
            }

            // Добавляем новый вариант ответа
            var option = new QuestionOption
            {
                QuestionId = questionId,
                OptionText = request.OptionText
            };

            await _context.Options.AddAsync(option);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Вариант ответа успешно добавлен.",
                optionId = option.Id
            });
        }

        // Метод для удаления вопроса
        [HttpDelete("{questionnaireId}/questions/{questionId}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int questionnaireId, int questionId)
        {
            // Проверяем права доступа
            var (userId, _) = await GetUserIdAndAnonymousIdAsync();

            // Находим вопрос по ID и ID анкеты
            var question = await _context.Questions
                .Include(q => q.Options) // Подключаем варианты ответов для удаления
                .FirstOrDefaultAsync(q => q.Id == questionId && q.QuestionnaireId == questionnaireId && q.Questionnaire.UserId == userId);

            if (question == null)
            {
                return NotFound("Вопрос не найден или не принадлежит указанной анкете.");
            }

            // Удаляем связанные ответы пользователей
            var answersToRemove = await _context.Answers
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();

            if (answersToRemove.Any())
            {
                _context.Answers.RemoveRange(answersToRemove);
            }

            // Удаляем связанные варианты ответов
            if (question.Options != null && question.Options.Any())
            {
                _context.Options.RemoveRange(question.Options);
            }

            // Удаляем сам вопрос
            _context.Questions.Remove(question);

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Вопрос успешно удален."
            });
        }
    }
}