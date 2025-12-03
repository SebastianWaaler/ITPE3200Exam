using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Data.Repositories.Interfaces;

namespace QuizApp.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository for Question database operations. Handles all data access for questions.
    /// Questions are the individual problems within a quiz, each having multiple answer options.
    /// </summary>
    public class QuestionRepository : IQuestionRepository
    {
        private readonly QuizContext _context;

        public QuestionRepository(QuizContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a single question by ID, including all its answer options and the parent quiz.
        /// Used when displaying, editing, or deleting a question.
        /// </summary>
        public async Task<Question?> GetByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        /// <summary>
        /// Retrieves all questions for a specific quiz, including their answer options.
        /// Used when displaying all questions in a quiz. Uses AsNoTracking for read-only performance.
        /// </summary>
        public async Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId)
        {
            return await _context.Questions
                .Where(q => q.QuizId == quizId)
                .Include(q => q.Options)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new question to the database. Used when creating a new question.
        /// The question should already have its answer options attached.
        /// </summary>
        public async Task AddAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing question in the database. Used when editing a question's text, points, or options.
        /// </summary>
        public async Task UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a question from the database. Also deletes all associated answer options (cascade delete).
        /// If the question doesn't exist, does nothing (no error).
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks if a question with the given ID exists in the database.
        /// Used for validation before updating or deleting.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Questions.AnyAsync(q => q.Id == id);
        }
    }
}
