using Microsoft.AspNetCore.Mvc;                                   // MVC base/types
using Microsoft.EntityFrameworkCore;                              // For DbUpdateConcurrencyException
using QuizApp.Models;                                             // Option, Question
using Microsoft.Extensions.Logging;                               // ILogger
using QuizApp.Data.Repositories.Interfaces;                            // IOptionRepository, IQuestionRepository

namespace QuizApp.Controllers
{
    public class OptionsController : Controller
    {
        private readonly IOptionRepository _options;              // Option repository
        private readonly IQuestionRepository _questions;          // Question repository
        private readonly ILogger<OptionsController> _logger;      // Logger for this controller

        public OptionsController(
            IOptionRepository options,
            IQuestionRepository questions,
            ILogger<OptionsController> logger)
        {
            _options = options;
            _questions = questions;
            _logger = logger;
        }

        public async Task<IActionResult> ByQuestion(int questionId) // List options for a question
        {
            try
            {
                // Load parent question (with Quiz, Options) via repository
                var question = await _questions.GetByIdAsync(questionId);

                if (question == null)
                {
                    _logger.LogWarning("Question {QuestionId} not found in ByQuestion.", questionId);
                    return NotFound();
                }

                // Load options list via repository (even though question already has Options, this shows repo usage clearly)
                var options = await _options.GetByQuestionIdAsync(questionId);

                ViewBag.Question = question;                            // Pass parent to view (title/breadcrumbs)
                return View(options);                                   // Render ByQuestion.cshtml
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading options for question {QuestionId}.", questionId);
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Create(int questionId)     // Show create form for an option under a question
        {
            try
            {
                var question = await _questions.GetByIdAsync(questionId);

                if (question == null)
                {
                    _logger.LogWarning("Question {QuestionId} not found in Create (GET).", questionId);
                    return NotFound();
                }

                ViewBag.Question = question;                             // For displaying context in the view
                return View(new Option { QuestionId = questionId });     // Pre-fill FK for binding
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing Create option form for question {QuestionId}.", questionId);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Text,IsCorrect,QuestionId")] Option option)
        {                                                            // Bind editable fields + FK
            try
            {
                var questionExists = await _questions.ExistsAsync(option.QuestionId); // Validate FK

                if (!questionExists)
                {
                    _logger.LogWarning("Question {QuestionId} not found in Create (POST).", option.QuestionId);
                    ModelState.AddModelError("", "Selected question does not exist.");
                }

                if (!ModelState.IsValid)                                 // Validation errors? redisplay
                {
                    ViewBag.Question = await _questions.GetByIdAsync(option.QuestionId);
                    return View(option);
                }

                await _options.AddAsync(option);                         // INSERT via repository

                _logger.LogInformation("Option {OptionId} created for Question {QuestionId}.", option.Id, option.QuestionId);

                return RedirectToAction(nameof(ByQuestion), new { questionId = option.QuestionId }); // Back to options list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating option for Question {QuestionId}.", option.QuestionId);
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Edit(int? id)               // Show edit form
        {
            if (id == null)
            {
                _logger.LogWarning("Edit called with null id.");
                return NotFound();
            }

            try
            {
                var option = await _options.GetByIdAsync(id.Value);  // includes Question + Quiz via repo

                if (option == null)
                {
                    _logger.LogWarning("Option {OptionId} not found in Edit (GET).", id);
                    return NotFound();
                }

                return View(option);                                 // Render Edit.cshtml
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Edit form for Option {OptionId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Text,IsCorrect,QuestionId")] Option option)
        {                                                            // Bind key + editable fields
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

                await _options.UpdateAsync(option);                  // UPDATE via repository

                _logger.LogInformation("Option {OptionId} updated for Question {QuestionId}.", option.Id, option.QuestionId);

                return RedirectToAction(nameof(ByQuestion), new { questionId = option.QuestionId }); // Back to options
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await _options.ExistsAsync(option.Id))
                {
                    _logger.LogWarning(ex, "Concurrency error: Option {OptionId} no longer exists.", option.Id);
                    return NotFound();                               // Deleted concurrently
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating Option {OptionId}.", option.Id);
                    throw;                                           // Let the global handler deal with it
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing Option {OptionId}.", option.Id);
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Delete(int? id)             // Confirm delete
        {
            if (id == null)
            {
                _logger.LogWarning("Delete (GET) called with null id.");
                return NotFound();
            }

            try
            {
                var option = await _options.GetByIdAsync(id.Value);  // includes Question + Quiz via repo

                if (option == null)
                {
                    _logger.LogWarning("Option {OptionId} not found in Delete (GET).", id);
                    return NotFound();
                }

                return View(option);                                  // Render Delete.cshtml
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing Delete confirmation for Option {OptionId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var option = await _options.GetByIdAsync(id);        // load via repo

                if (option == null)
                {
                    _logger.LogWarning("Option {OptionId} not found in DeleteConfirmed.", id);
                    return NotFound();
                }

                var questionId = option.QuestionId;                  // Save FK for redirect

                await _options.DeleteAsync(id);                      // DELETE via repository

                _logger.LogInformation("Option {OptionId} deleted for Question {QuestionId}.", id, questionId);

                return RedirectToAction(nameof(ByQuestion), new { questionId }); // Back to options list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Option {OptionId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
