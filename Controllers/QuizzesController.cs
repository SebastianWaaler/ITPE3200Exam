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
    [Authorize] // default: must be logged in
    public class QuizController : Controller
    {
        private readonly IQuizRepository _quizzes;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IQuizRepository quizzes, ILogger<QuizController> logger)
        {
            _quizzes = quizzes;
            _logger = logger;
        }

        // PUBLIC: everyone can see quiz list
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

        // ADMIN: quiz details with questions/options (admin management view)
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

        // ADMIN: create quiz (view)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // ADMIN: create quiz (fallback MVC POST – even if you mostly use API)
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

        // ADMIN: edit quiz (view)
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

        // ADMIN: edit quiz (fallback MVC POST – your Vue/API also protects)
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

        // ADMIN: delete quiz (confirm view)
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

        // ADMIN: delete quiz (POST)
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

        // LOGGED-IN USERS ONLY: Take quiz
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

        // LOGGED-IN USERS ONLY: Submit answers
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

                foreach (var question in quiz.Questions)
                {
                    totalPoints += question.Points;
                    string formKey = "question_" + question.Id;

                    if (!Request.Form.ContainsKey(formKey))
                        continue;

                    int selectedOptionId = int.Parse(Request.Form[formKey]!);
                    var selectedOption = question.Options.First(o => o.Id == selectedOptionId);
                    if (selectedOption.IsCorrect)
                    {
                        earnedPoints += question.Points;
                    }
                }

                var result = new QuizResultViewModel
                {
                    QuizTitle = quiz.Title,
                    TotalPoints = totalPoints,
                    EarnedPoints = earnedPoints
                };

                return View("QuizResult", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz {QuizId}", QuizId);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
