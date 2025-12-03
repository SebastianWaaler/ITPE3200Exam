using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuizApp.Data.Repositories.Interfaces;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]  // -> /api/QuizApi
    public class QuizApiController : ControllerBase
    {
        private readonly IQuizRepository _quizzes;

        public QuizApiController(IQuizRepository quizzes)
        {
            _quizzes = quizzes;
        }

        // GET: /api/QuizApi
        // Return all quizzes (for Index page)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var quizzes = await _quizzes.GetAllAsync();

            // You can return entities directly if they don't create cycles
            // or project to a DTO if you prefer.
            var dto = quizzes.Select(q => new
            {
                quizId = q.QuizId,
                title = q.Title,
                description = q.Description
            });

            return Ok(dto);
        }

        // GET: /api/QuizApi/5
        // Single quiz including its questions/options (your GetByIdAsync already loads them)
        [HttpGet("{id}")]
        [Authorize]   // you can remove this if you want anonymous Take
        public async Task<IActionResult> Get(int id)
        {
            var quiz = await _quizzes.GetByIdAsync(id);
            if (quiz == null)
                return NotFound();

            var dto = new
            {
                quizId = quiz.QuizId,
                title = quiz.Title,
                description = quiz.Description,
                questions = quiz.Questions.Select(q => new
                {
                    id = q.Id,
                    text = q.Text,
                    points = q.Points,
                    options = q.Options.Select(o => new
                    {
                        id = o.Id,
                        text = o.Text,
                        isCorrect = o.IsCorrect
                    })
                })
            };

            return Ok(dto);
        }

        // POST: /api/QuizApi
        // Create a new quiz (used by Create page if you go full-API there)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Quiz quiz)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _quizzes.AddAsync(quiz);

            // CreatedAtAction needs the correct route and key name
            return CreatedAtAction(
                nameof(Get),
                new { id = quiz.QuizId },
                new
                {
                    quizId = quiz.QuizId,
                    title = quiz.Title,
                    description = quiz.Description
                });
        }

        // PUT: /api/QuizApi/5
        // Update basic quiz info (Title, Description)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Quiz quiz)
        {
            if (id != quiz.QuizId)
                return BadRequest("Route id and body QuizId do not match.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _quizzes.ExistsAsync(id))
                return NotFound();

            await _quizzes.UpdateAsync(quiz);
            return NoContent();
        }

        // DELETE: /api/QuizApi/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _quizzes.ExistsAsync(id))
                return NotFound();

            await _quizzes.DeleteAsync(id);
            return NoContent();
        }
    }
}
