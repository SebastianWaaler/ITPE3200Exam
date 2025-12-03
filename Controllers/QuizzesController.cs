using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizApp.Data.Repositories.Interfaces;
using QuizApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
    /// <summary>
    /// Controller for managing quizzes. Handles displaying quiz lists, creating/editing/deleting quizzes (admin only),
    /// and allowing users to take quizzes and submit answers.
    /// </summary>
    [Authorize]
    public class QuizController : Controller
    {
        private readonly IQuizRepository _quizzes;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IQuizRepository quizzes, ILogger<QuizController> logger)
        {
            _quizzes = quizzes;
            _logger = logger;
        }

        /// <summary>
        /// Displays the main quiz list page. Shows all available quizzes that users can take.
        /// Anyone can view this page (no login required).
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var quizzes = await _quizzes.GetAllAsync();
                return View(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quiz list.");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows detailed view of a quiz for admins. Displays the quiz title, description, and all questions
        /// with their answer options. Admins can see which options are marked as correct.
        /// Used for managing quiz content.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var quiz = await _quizzes.GetByIdAsync(id.Value);
                if (quiz == null) return NotFound();

                return View(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Details for quiz {QuizId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the form for creating a new quiz. Admin only.
        /// The form allows entering a title and description for the quiz.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Saves a newly created quiz to the database. Admin only.
        /// After creation, redirects to the quiz list page.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Description")] Quiz quiz)
        {
            if (!ModelState.IsValid) return View(quiz);

            try
            {
                await _quizzes.AddAsync(quiz);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz.");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the edit form for an existing quiz. Admin only.
        /// Pre-fills the form with the current quiz title and description.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var quiz = await _quizzes.GetByIdAsync(id.Value);
                if (quiz == null) return NotFound();

                return View(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Edit form for quiz {QuizId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Updates an existing quiz's title and description in the database. Admin only.
        /// After updating, redirects to the quiz list page.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("QuizId,Title,Description")] Quiz quiz)
        {
            if (id != quiz.QuizId) return NotFound();
            if (!ModelState.IsValid) return View(quiz);

            try
            {
                await _quizzes.UpdateAsync(quiz);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error editing quiz {QuizId}", quiz.QuizId);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing quiz {QuizId}", quiz.QuizId);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the confirmation page before deleting a quiz. Admin only.
        /// Displays the quiz title so the admin can confirm they want to delete it.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var quiz = await _quizzes.GetByIdAsync(id.Value);
                if (quiz == null) return NotFound();

                return View(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Delete view for quiz {QuizId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Permanently deletes a quiz from the database. Admin only.
        /// This will also delete all questions and options associated with the quiz.
        /// After deletion, redirects to the quiz list page.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _quizzes.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quiz {QuizId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Displays the quiz-taking page. Shows all questions with their answer options as radio buttons.
        /// Users must be logged in to take a quiz. The correct answers are hidden from users.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Take(int id)
        {
            try
            {
                var quiz = await _quizzes.GetByIdAsync(id);
                if (quiz == null) return NotFound();

                return View(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Take view for quiz {QuizId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Processes the submitted quiz answers and calculates the score.
        /// Compares each selected answer against the correct answer for that question.
        /// Awards points for each correct answer based on the question's point value.
        /// Returns a results page showing the score (earned points / total points) and percentage.
        /// </summary>
        /// <summary>
        /// Processes the submitted quiz answers and calculates the score.
        /// Returns JSON result for SPA client-side navigation.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Submit(int QuizId)
        {
            try
            {
                var quiz = await _quizzes.GetByIdAsync(QuizId);
                if (quiz == null) return NotFound();

                int totalPoints = 0;
                int earnedPoints = 0;

                // Go through each question and check if the user's selected answer is correct
                foreach (var question in quiz.Questions)
                {
                    totalPoints += question.Points;
                    string formKey = "question_" + question.Id;

                    // Skip if user didn't answer this question
                    if (!Request.Form.ContainsKey(formKey))
                        continue;

                    int selectedOptionId = int.Parse(Request.Form[formKey]!);
                    var selectedOption = question.Options.First(o => o.Id == selectedOptionId);
                    if (selectedOption.IsCorrect)
                    {
                        earnedPoints += question.Points;
                    }
                }

                // Return JSON for SPA client-side navigation
                return Json(new
                {
                    quizTitle = quiz.Title,
                    totalPoints = totalPoints,
                    earnedPoints = earnedPoints
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz {QuizId}", QuizId);
                return StatusCode(500, new { error = "Failed to submit quiz" });
            }
        }
    }
}
