using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Data.Repositories.Interfaces;

namespace QuizApp.Controllers
{
    /// <summary>
    /// REST API controller for question operations. Used by the SPA for question management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class QuestionApiController : ControllerBase
    {
        private readonly IQuestionRepository _questions;

        public QuestionApiController(IQuestionRepository questions)
        {
            _questions = questions;
        }

        /// <summary>
        /// Returns a single question by ID with its options.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var question = await _questions.GetByIdAsync(id);
            if (question == null)
                return NotFound();

            var dto = new
            {
                id = question.Id,
                quizId = question.QuizId,
                text = question.Text,
                points = question.Points,
                options = question.Options.Select(o => new
                {
                    id = o.Id,
                    text = o.Text,
                    isCorrect = o.IsCorrect
                })
            };

            return Ok(dto);
        }
    }
}

