using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Data.Repositories.Interfaces;

namespace QuizApp.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository for Quiz database operations. Handles all data access for quizzes.
    /// Implements the repository pattern to separate data access logic from business logic.
    /// </summary>
    public class QuizRepository : IQuizRepository
    {
        private readonly QuizContext _context;

        public QuizRepository(QuizContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all quizzes from the database. Used by the quiz list page.
        /// Uses AsNoTracking for better performance since we're only reading the data.
        /// </summary>
        public async Task<IEnumerable<Quiz>> GetAllAsync()
        {
            return await _context.Quizzes.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Retrieves a single quiz by ID, including all its questions and their answer options.
        /// Used when displaying quiz details, taking a quiz, or editing a quiz.
        /// Uses eager loading (Include) to load related data in one database query.
        /// </summary>
        public async Task<Quiz?> GetByIdAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuizId == id);
        }

        /// <summary>
        /// Adds a new quiz to the database. Used when creating a new quiz.
        /// </summary>
        public async Task AddAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing quiz in the database. Used when editing a quiz's title or description.
        /// </summary>
        public async Task UpdateAsync(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a quiz from the database. Also deletes all associated questions and options (cascade delete).
        /// If the quiz doesn't exist, does nothing (no error).
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks if a quiz with the given ID exists in the database.
        /// Used for validation before updating or deleting.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Quizzes.AnyAsync(q => q.QuizId == id);
        }
    }
}
