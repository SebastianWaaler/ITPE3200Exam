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

        // GET: /Questions/Details/5
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

        // GET: /Questions/Create?quizId=5
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

        // POST: /Questions/Create
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

                // Clean options: remove empty options and trim whitespace
                question.Options = question.Options
                    .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                    .Select(o => new Option { Text = o.Text.Trim(), IsCorrect = false })
                    .ToList();

                // Server-side validation: require at least 2 options
                if (question.Options.Count < 2)
                    ModelState.AddModelError("", "At least two options required.");

                // Server-side validation: require a correct answer to be selected
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

        // GET: /Questions/Edit/5
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

        // POST: /Questions/Edit/5
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

                // Update question fields
                question.Text = formQuestion.Text?.Trim() ?? "";
                question.Points = formQuestion.Points;

                Options ??= new List<Option>();

                // Clean options: remove empty options and trim whitespace
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

                // Server-side validation: require at least 2 options
                if (cleanedOptions.Count < 2)
                    ModelState.AddModelError("", "A question must have at least 2 answer options.");

                // Server-side validation: require a correct answer to be selected
                if (CorrectIndex == null || CorrectIndex < 0 || CorrectIndex >= cleanedOptions.Count)
                    ModelState.AddModelError("", "Select which answer is correct.");
                else
                    cleanedOptions[(int)CorrectIndex].IsCorrect = true;

                if (!ModelState.IsValid)
                    return View(question);

                // Deleted options
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

                // Add/update options
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

        // GET: /Questions/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _questions.GetByIdAsync(id);
            if (question == null) return NotFound();

            return View(question);
        }

        // POST: /Questions/Delete/5
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
