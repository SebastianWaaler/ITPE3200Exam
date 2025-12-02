using Microsoft.AspNetCore.Mvc;                               // Brings in MVC attributes and base classes like Controller
using Microsoft.EntityFrameworkCore;                          // For DbUpdateConcurrencyException
using QuizApp.Models;                                         // Gives access to your Quiz, Question, Option models
using System.Threading.Tasks;                                 // For Task / async support
using System.Linq;
using Microsoft.Extensions.Logging;                           // For ILogger<T>
using QuizApp.Data.Repositories.Interfaces;                        // For IQuizRepository

namespace QuizApp.Controllers
{
    public class QuizController : Controller
    {
        private readonly IQuizRepository _quizzes;            // Repository instead of DbContext
        private readonly ILogger<QuizController> _logger;      // Logger for this controller

        public QuizController(IQuizRepository quizzes, ILogger<QuizController> logger)
        {
            _quizzes = quizzes;
            _logger = logger;
        }

        // GET Quizzes
        public async Task<IActionResult> Index()                // Action method that returns a view listing all quizzes
        {
            try
            {
                var quizzes = await _quizzes.GetAllAsync();    // via repository
                return View(quizzes);                          // Pass the list to the Index.cshtml view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quiz list in Index.");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null!)
            {
                _logger.LogWarning("Details called with null id.");
                return NotFound();
            }

            try
            {
                var quiz = await _quizzes.GetByIdAsync(id.Value); // repository returns quiz with questions + options

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz {QuizId} not found in Details.", id);
                    return NotFound();
                }

                return View(quiz);                             // Render Details.cshtml with the quiz model
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Details for Quiz {QuizId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: /Quizzes/Create
        public IActionResult Create()                         // Shows the empty create form
        {
            return View();                                    // Returns Create.cshtml (strongly-typed to Quiz)
        }

        // POST: /Quizzes/Create/
        [HttpPost]                                            // This action handles form POST requests
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title, Description")] Quiz quiz) // The [Bind] limits which properties are bound from the form
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _quizzes.AddAsync(quiz);            // via repository

                    _logger.LogInformation("Quiz {QuizId} created.", quiz.QuizId);

                    return RedirectToAction(nameof(Index));
                }

                return View(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz.");
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: /Quizzes/Edit/
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit (GET) called with null id.");
                return NotFound();
            }

            try
            {
                var quiz = await _quizzes.GetByIdAsync(id.Value); // get via repository
                if (quiz == null)
                {
                    _logger.LogWarning("Quiz {QuizId} not found in Edit (GET).", id);
                    return NotFound();
                }

                return View("~/Views/Quiz/Edit.cshtml", quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Edit form for Quiz {QuizId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: /Quizzes/Edit/5
        [HttpPost]                                            // Handles POST for edit
        [ValidateAntiForgeryToken]                            // Protects against CSRF
        public async Task<IActionResult> Edit(int id, [Bind("QuizId,Title,Description")] Quiz quiz) // We bind key + editable fields (not Questions list)
        {
            if (id != quiz.QuizId)                            // Route id must match form’s key value
            {
                _logger.LogWarning("Edit (POST) called with mismatched id. Route id: {RouteId}, Model id: {ModelId}", id, quiz.QuizId);
                return NotFound();
            }

            if (!ModelState.IsValid)                          // If validation failed, redisplay form
                return View(quiz);

            try
            {
                await _quizzes.UpdateAsync(quiz);             // via repository

                _logger.LogInformation("Quiz {QuizId} updated.", quiz.QuizId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await _quizzes.ExistsAsync(quiz.QuizId))
                {
                    _logger.LogWarning(ex, "Concurrency error: Quiz {QuizId} no longer exists.", quiz.QuizId);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating Quiz {QuizId}.", quiz.QuizId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing Quiz {QuizId}.", quiz.QuizId);
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Quiz/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete (GET) called with null id.");
                return NotFound();
            }

            try
            {
                var quiz = await _quizzes.GetByIdAsync(id.Value); // via repository

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz {QuizId} not found in Delete (GET).", id);
                    return NotFound();
                }

                return View(quiz);  // Views/Quiz/Delete.cshtml
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing Delete confirmation for Quiz {QuizId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var quiz = await _quizzes.GetByIdAsync(id);   // include children via repo

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz {QuizId} not found in DeleteConfirmed.", id);
                    return NotFound();
                }

                await _quizzes.DeleteAsync(id);               // via repository

                _logger.LogInformation("Quiz {QuizId} deleted.", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Quiz {QuizId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Take(int id)
        {
            try
            {
                var quiz = await _quizzes.GetByIdAsync(id);   // repo: includes questions + options

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz {QuizId} not found in Take.", id);
                    return NotFound();
                }

                return View(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Take view for Quiz {QuizId}.", id);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int QuizId)
        {
            try
            {
                // Load quiz including questions and options via repository
                var quiz = await _quizzes.GetByIdAsync(QuizId);

                if (quiz == null)
                {
                    _logger.LogWarning("Quiz {QuizId} not found in Submit.", QuizId);
                    return NotFound();
                }

                int totalPoints = 0;
                int earnedPoints = 0;

                foreach (var question in quiz.Questions)
                {
                    totalPoints += question.Points;

                    // Name of radio button group: question_QUESTIONID
                    string formKey = $"question_{question.Id}";

                    // Did the user answer this question?
                    if (!Request.Form.ContainsKey(formKey))
                        continue;

                    int selectedOptionId = int.Parse(Request.Form[formKey]!);

                    var selectedOption =
                        question.Options.First(o => o.Id == selectedOptionId);

                    if (selectedOption.IsCorrect)
                    {
                        earnedPoints += question.Points;
                    }
                }

                // Pass results to the view
                var result = new QuizResultViewModel
                {
                    QuizTitle = quiz.Title,
                    TotalPoints = totalPoints,
                    EarnedPoints = earnedPoints
                };

                _logger.LogInformation("Quiz {QuizId} submitted. Score: {Earned}/{Total}.",
                    QuizId, earnedPoints, totalPoints);

                return View("QuizResult", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting Quiz {QuizId}.", QuizId);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
