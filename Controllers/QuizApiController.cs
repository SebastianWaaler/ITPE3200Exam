using Microsoft.AspNetCore.Mvc;
using QuizApp.Data.Repositories.Interfaces;   // adjust to your actual namespace
using Microsoft.Extensions.Logging;

namespace QuizApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizApiController : ControllerBase
    {
        private readonly IQuizRepository _quizzes;
        private readonly ILogger<QuizApiController> _logger;

        public QuizApiController(IQuizRepository quizzes, ILogger<QuizApiController> logger)
        {
            _quizzes = quizzes;
            _logger = logger;
        }

        // GET /api/quizapi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuiz(int id)
        {
            try
            {
                var quiz = await _quizzes.GetByIdAsync(id);

                if (quiz == null)
                    return NotFound();

                // Project to a JSON-friendly DTO to avoid circular refs
                var dto = new
                {
                    quiz.QuizId,
                    quiz.Title,
                    quiz.Description,
                    Questions = quiz.Questions.Select(q => new
                    {
                        q.Id,
                        q.Text,
                        q.Points,
                        Options = q.Options.Select(o => new
                        {
                            o.Id,
                            o.Text,
                            o.IsCorrect
                        })
                    })
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QuizApiController.GetQuiz({Id})", id);
                return StatusCode(500, "Server error");
            }
        }
    }
}
