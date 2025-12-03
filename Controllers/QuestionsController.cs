using Microsoft.AspNetCore.Mvc;
using QuizApp.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QuizApp.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QuizApp.Controllers
{
    /// <summary>
    /// Controller for managing questions within quizzes. Admin only.
    /// Questions are the individual problems in a quiz, each with multiple answer options.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("Questions")]
    public class QuestionsController : Controller
    {
        private readonly IQuestionRepository _questions;
        private readonly IQuizRepository _quizzes;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(
            IQuestionRepository questions,
            IQuizRepository quizzes,
            ILogger<QuestionsController> logger)
        {
            _questions = questions;
            _quizzes = quizzes;
            _logger = logger;
        }

        /// <summary>
        /// Shows details of a specific question including its text, point value, and all answer options.
        /// Used by admins to view question information.
        /// </summary>
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var question = await _questions.GetByIdAsync(id);
                if (question == null) return NotFound();

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading details for Question {QuestionId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the form for creating a new question for a specific quiz.
        /// The form allows entering the question text, point value, and multiple answer options.
        /// At least 2 options are required, and one must be marked as correct.
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int quizId)
        {
            try
            {
                var quiz = await _quizzes.GetByIdAsync(quizId);
                if (quiz == null) return NotFound();

                ViewBag.Quiz = quiz;

                return View(new Question
                {
                    QuizId = quizId,
                    Points = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading question creation form.");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Saves a newly created question to the database.
        /// Validates that the question has at least 2 answer options and that one is marked as correct.
        /// The CorrectIndex parameter indicates which option (by position) is the correct answer.
        /// After creation, redirects back to the quiz details page.
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question, int? CorrectIndex)
        {
            try
            {
                var quiz = await _quizzes.GetByIdAsync(question.QuizId);
                if (quiz == null)
                {
                    ModelState.AddModelError("", "Quiz not found.");
                }
                else
                {
                    question.Quiz = quiz;
                }

                question.Options ??= new List<Option>();

                // Remove empty options and trim text
                question.Options = question.Options
                    .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                    .Select(o => new Option { Text = o.Text.Trim(), IsCorrect = false })
                    .ToList();

                if (question.Options.Count < 2)
                    ModelState.AddModelError("", "At least two options required.");

                // Make sure a correct answer is selected
                if (CorrectIndex == null || CorrectIndex < 0 || CorrectIndex >= question.Options.Count)
                    ModelState.AddModelError("", "Select a correct answer.");
                else
                    question.Options[(int)CorrectIndex].IsCorrect = true;

                if (!ModelState.IsValid)
                {
                    ViewBag.Quiz = quiz;
                    return View(question);
                }

                foreach (var o in question.Options)
                    o.Question = question;

                await _questions.AddAsync(question);

                return RedirectToAction(nameof(QuizController.Details), "Quiz", new { id = question.QuizId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating question");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the edit form for an existing question.
        /// Pre-fills the form with the current question text, points, and all answer options.
        /// </summary>
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var question = await _questions.GetByIdAsync(id);
                if (question == null) return NotFound();

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for Question {QuestionId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Updates an existing question in the database.
        /// Allows changing the question text, point value, and answer options.
        /// Can add new options, update existing ones, or delete options (via DeletedOptionIds).
        /// The CorrectIndex parameter indicates which option is the correct answer.
        /// Validates that at least 2 options remain and one is marked as correct.
        /// After updating, redirects back to the quiz details page.
        /// </summary>
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Question formQuestion,
            List<Option>? Options,
            string? DeletedOptionIds,
            int? CorrectIndex)
        {
            if (id != formQuestion.Id)
                return NotFound();

            try
            {
                var question = await _questions.GetByIdAsync(id);
                if (question == null) return NotFound();

                question.Text = formQuestion.Text?.Trim() ?? "";
                question.Points = formQuestion.Points;

                Options ??= new List<Option>();

                // Filter out empty options and reset IsCorrect flags
                var cleanedOptions = Options
                    .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                    .Select(o => new Option
                    {
                        Id = o.Id,
                        Text = o.Text.Trim(),
                        IsCorrect = false,
                        QuestionId = question.Id
                    })
                    .ToList();

                if (cleanedOptions.Count < 2)
                    ModelState.AddModelError("", "A question must have at least 2 answer options.");

                if (CorrectIndex == null || CorrectIndex < 0 || CorrectIndex >= cleanedOptions.Count)
                    ModelState.AddModelError("", "Select which answer is correct.");
                else
                    cleanedOptions[(int)CorrectIndex].IsCorrect = true;

                if (!ModelState.IsValid)
                    return View(question);

                // Handle deleted options
                var deletedIds = new List<int>();
                if (!string.IsNullOrWhiteSpace(DeletedOptionIds))
                {
                    deletedIds = DeletedOptionIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                }

                foreach (var deleteId in deletedIds)
                {
                    var delOpt = question.Options.FirstOrDefault(o => o.Id == deleteId);
                    if (delOpt != null)
                        question.Options.Remove(delOpt);
                }

                // Add new options or update existing ones
                foreach (var opt in cleanedOptions)
                {
                    if (opt.Id == 0)
                    {
                        question.Options.Add(new Option
                        {
                            Text = opt.Text,
                            IsCorrect = opt.IsCorrect,
                            QuestionId = question.Id
                        });
                    }
                    else
                    {
                        var existing = question.Options.First(o => o.Id == opt.Id);
                        existing.Text = opt.Text;
                        existing.IsCorrect = opt.IsCorrect;
                    }
                }

                await _questions.UpdateAsync(question);

                return RedirectToAction(nameof(QuizController.Details), "Quiz", new { id = question.QuizId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing Question {QuestionId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the confirmation page before deleting a question.
        /// Displays the question text so the admin can confirm they want to delete it.
        /// </summary>
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _questions.GetByIdAsync(id);
            if (question == null) return NotFound();

            return View(question);
        }

        /// <summary>
        /// Permanently deletes a question from the database.
        /// This will also delete all answer options associated with the question.
        /// After deletion, redirects back to the quiz details page.
        /// </summary>
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var question = await _questions.GetByIdAsync(id);
                if (question == null) return NotFound();

                var quizId = question.QuizId;

                await _questions.DeleteAsync(id);

                return RedirectToAction(nameof(QuizController.Details), "Quiz", new { id = quizId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Question {QuestionId}", id);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
