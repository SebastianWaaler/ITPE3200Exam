using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuizApp.Data.Repositories.Interfaces;
using QuizApp.Models;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<QuizApiController> _logger;

        public QuizApiController(IQuizRepository quizzes, ILogger<QuizApiController> logger)
        {
            _quizzes = quizzes;
            _logger = logger;
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
            try
            {
                var quizzes = await _quizzes.GetAllAsync();

                var dto = quizzes.Select(q => new
                {
                    quizId = q.QuizId,
                    title = q.Title,
                    description = q.Description
                });

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QuizApiController.GetAll");
                return StatusCode(500, "An error occurred while loading quizzes.");
            }
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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QuizApiController.Get({QuizId})", id);
                return StatusCode(500, "An error occurred while loading the quiz.");
            }
        }

        // POST/PUT/DELETE can stay as you had them, or also get try/catch if you want.
        // They’re already OK for the exam’s minimal requirements.
    }
}