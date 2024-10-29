using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using questionnaire.Models;

namespace questionnaire.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionnaireController : ControllerBase
    {
        private readonly questionnaireContext _context;
        public QuestionnaireController(questionnaireContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Questionnaire>>> GetProducts()
        {
            var questionnaires = await _context.Questionnaires.ToListAsync();
            return Ok(questionnaires);
        }

    }

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly questionnaireContext _context;

        public UserController(questionnaireContext context)
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
}
