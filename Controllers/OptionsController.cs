using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizApp.Data.Repositories.Interfaces;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    /// <summary>
    /// Controller for managing answer options within questions. Admin only.
    /// Options are the possible answers for a question. Each question has multiple options, with one marked as correct.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OptionsController : Controller
    {
        private readonly IOptionRepository _options;
        private readonly IQuestionRepository _questions;
        private readonly ILogger<OptionsController> _logger;

        public OptionsController(
            IOptionRepository options,
            IQuestionRepository questions,
            ILogger<OptionsController> logger)
        {
            _options = options;
            _questions = questions;
            _logger = logger;
        }

        /// <summary>
        /// Shows all answer options for a specific question. Displays a list of options with their text and correct status.
        /// Used by admins to view and manage all options for a question.
        /// </summary>
        public async Task<IActionResult> ByQuestion(int questionId)
        {
            try
            {
                var question = await _questions.GetByIdAsync(questionId);

                if (question == null)
                {
                    _logger.LogWarning("Question {QuestionId} not found in ByQuestion.", questionId);
                    return NotFound();
                }

                var options = await _options.GetByQuestionIdAsync(questionId);

                ViewBag.Question = question;
                return View(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading options for question {QuestionId}.", questionId);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the form for creating a new answer option for a specific question.
        /// The form allows entering the option text and marking it as correct or incorrect.
        /// </summary>
        public async Task<IActionResult> Create(int questionId)
        {
            try
            {
                var question = await _questions.GetByIdAsync(questionId);

                if (question == null)
                {
                    _logger.LogWarning("Question {QuestionId} not found in Create (GET).", questionId);
                    return NotFound();
                }

                ViewBag.Question = question;
                return View(new Option { QuestionId = questionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing Create option form for question {QuestionId}.", questionId);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Saves a newly created answer option to the database.
        /// Validates that the parent question exists.
        /// After creation, redirects back to the options list for that question.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Text,IsCorrect,QuestionId")] Option option)
        {
            try
            {
                var questionExists = await _questions.ExistsAsync(option.QuestionId);

                if (!questionExists)
                {
                    _logger.LogWarning("Question {QuestionId} not found in Create (POST).", option.QuestionId);
                    ModelState.AddModelError("", "Selected question does not exist.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Question = await _questions.GetByIdAsync(option.QuestionId);
                    return View(option);
                }

                await _options.AddAsync(option);

                _logger.LogInformation("Option {OptionId} created for Question {QuestionId}.", option.Id, option.QuestionId);

                return RedirectToAction(nameof(ByQuestion), new { questionId = option.QuestionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating option for Question {QuestionId}.", option.QuestionId);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the edit form for an existing answer option.
        /// Pre-fills the form with the current option text and correct status.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit called with null id.");
                return NotFound();
            }

            try
            {
                var option = await _options.GetByIdAsync(id.Value);

                if (option == null)
                {
                    _logger.LogWarning("Option {OptionId} not found in Edit (GET).", id);
                    return NotFound();
                }

                return View(option);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Edit form for Option {OptionId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Updates an existing answer option in the database.
        /// Allows changing the option text and whether it's marked as correct.
        /// Validates that the parent question exists.
        /// After updating, redirects back to the options list for that question.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Text,IsCorrect,QuestionId")] Option option)
        {
            if (id != option.Id)
            {
                _logger.LogWarning("Edit (POST) called with mismatched id. Route id: {RouteId}, Model id: {ModelId}", id, option.Id);
                return NotFound();
            }

            try
            {
                if (!await _questions.ExistsAsync(option.QuestionId))
                {
                    _logger.LogWarning("Question {QuestionId} not found in Edit (POST).", option.QuestionId);
                    ModelState.AddModelError("", "Selected question does not exist.");
                }

                if (!ModelState.IsValid)
                    return View(option);

                await _options.UpdateAsync(option);

                _logger.LogInformation("Option {OptionId} updated for Question {QuestionId}.", option.Id, option.QuestionId);

                return RedirectToAction(nameof(ByQuestion), new { questionId = option.QuestionId });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await _options.ExistsAsync(option.Id))
                {
                    _logger.LogWarning(ex, "Concurrency error: Option {OptionId} no longer exists.", option.Id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating Option {OptionId}.", option.Id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing Option {OptionId}.", option.Id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Shows the confirmation page before deleting an answer option.
        /// Displays the option text so the admin can confirm they want to delete it.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete (GET) called with null id.");
                return NotFound();
            }

            try
            {
                var option = await _options.GetByIdAsync(id.Value);

                if (option == null)
                {
                    _logger.LogWarning("Option {OptionId} not found in Delete (GET).", id);
                    return NotFound();
                }

                return View(option);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing Delete confirmation for Option {OptionId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Permanently deletes an answer option from the database.
        /// After deletion, redirects back to the options list for the parent question.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var option = await _options.GetByIdAsync(id);

                if (option == null)
                {
                    _logger.LogWarning("Option {OptionId} not found in DeleteConfirmed.", id);
                    return NotFound();
                }

                var questionId = option.QuestionId;

                await _options.DeleteAsync(id);

                _logger.LogInformation("Option {OptionId} deleted for Question {QuestionId}.", id, questionId);

                return RedirectToAction(nameof(ByQuestion), new { questionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Option {OptionId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
