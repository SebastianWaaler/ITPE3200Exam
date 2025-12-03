using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuizApp.Data.Repositories.Interfaces;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    /// <summary>
    /// REST API controller for quiz operations. Used by client-side JavaScript (Vue.js) to fetch and manipulate quiz data.
    /// Provides JSON endpoints for the quiz list page, quiz taking page, and admin quiz management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuizApiController : ControllerBase
    {
        private readonly IQuizRepository _quizzes;

        public QuizApiController(IQuizRepository quizzes)
        {
            _quizzes = quizzes;
        }

        /// <summary>
        /// Returns a list of all quizzes as JSON. Used by the quiz list page to display available quizzes.
        /// Returns only basic quiz information (id, title, description) to avoid circular references.
        /// Anyone can access this endpoint (no login required).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var quizzes = await _quizzes.GetAllAsync();

            // Return simplified DTO to avoid circular references
            var dto = quizzes.Select(q => new
            {
                quizId = q.QuizId,
                title = q.Title,
                description = q.Description
            });

            return Ok(dto);
        }

        /// <summary>
        /// Returns a single quiz with all its questions and answer options as JSON.
        /// Used by the quiz-taking page to load the full quiz content.
        /// Includes the isCorrect flag for each option (used server-side for scoring, but hidden from users in the UI).
        /// Requires user to be logged in.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
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

        /// <summary>
        /// Creates a new quiz via API. Used by the admin quiz creation page.
        /// Accepts quiz data as JSON in the request body.
        /// Returns the created quiz with its generated ID.
        /// Admin only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Quiz quiz)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _quizzes.AddAsync(quiz);

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

        /// <summary>
        /// Updates an existing quiz's title and description via API.
        /// Used by the admin quiz edit page.
        /// Accepts quiz data as JSON in the request body.
        /// Admin only.
        /// </summary>
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

        /// <summary>
        /// Deletes a quiz via API. Used by the admin quiz deletion page.
        /// Permanently removes the quiz and all its questions and options from the database.
        /// Admin only.
        /// </summary>
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
