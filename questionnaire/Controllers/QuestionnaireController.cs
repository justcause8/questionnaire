using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using questionnaire.Models;

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

    //[Route("questionnaires")]
    //[ApiController]
    //public class QuestionnaireController : ControllerBase
    //{
    //    private readonly questionnaireContext _context;

    //    public QuestionnaireController(questionnaireContext context)
    //    {
    //        _context = context;
    //    }

    //    [HttpGet]
    //    [CustomAuthorization]
    //    public async Task<ActionResult<List<Questionnaire>>> GetQuestionnaires()
    //    {
    //        var questionnaires = await _context.Questionnaires.ToListAsync();
    //        return Ok(questionnaires);
    //    }

    //    [CustomAuthorization]
    //    [HttpPost("create")]
    //    public async Task<ActionResult> CreateQuestionnaire([FromBody] Questionnaire model)
    //    {
    //        // Получаем ID авторизованного пользователя из контекста
    //        var userIdFromToken = HttpContext.Items["userId"]?.ToString();

    //        if (string.IsNullOrEmpty(userIdFromToken))
    //        {
    //            return Unauthorized("User ID not found in the token.");
    //        }

    //        if (model.UserId.ToString() != userIdFromToken)
    //        {
    //            return Forbid("You can only create questionnaires for your own user account.");
    //        }

    //        // Проверяем, существует ли пользователь с переданным UserId
    //        var user = await _context.Users.FindAsync(model.UserId);
    //        if (user == null)
    //        {
    //            return NotFound("User not found.");
    //        }

    //        // Проверяем допустимость типа анкеты
    //        if (model.TypeQuestionnaireId != 1 && model.TypeQuestionnaireId != 2)
    //        {
    //            return BadRequest("Invalid TypeQuestionnaireId. Allowed values: 1 (open), 2 (closed).");
    //        }

    //        // Создаем новую анкету
    //        var questionnaire = new Questionnaire
    //        {
    //            Title = model.Title,
    //            UserId = model.UserId, // Используем ID из тела запроса
    //            DesignId = 1,
    //            TypeQuestionnaireId = model.TypeQuestionnaireId
    //        };

    //        // Добавляем анкету в БД
    //        _context.Questionnaires.Add(questionnaire);
    //        await _context.SaveChangesAsync();

    //        return Ok(new { Message = "Questionnaire created successfully.", QuestionnaireId = questionnaire.Id });
    //    }
    //}
}
