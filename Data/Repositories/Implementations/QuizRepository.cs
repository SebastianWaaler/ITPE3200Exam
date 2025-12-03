using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Data.Repositories.Interfaces;

namespace QuizApp.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Quiz entity using Entity Framework Core.
    /// Implements the repository pattern to separate data access logic from business logic.
    /// All database operations are asynchronous for better performance.
    /// </summary>
    public class QuizRepository : IQuizRepository
    {
        private readonly QuizContext _context;

        public QuizRepository(QuizContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all quizzes from the database asynchronously.
        /// Uses AsNoTracking for read-only operations to improve performance.
        /// </summary>
        public async Task<IEnumerable<Quiz>> GetAllAsync()
        {
            return await _context.Quizzes.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Retrieves a quiz by ID including related questions and options.
        /// Returns null if the quiz is not found.
        /// </summary>
        public async Task<Quiz?> GetByIdAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuizId == id);
        }

        /// <summary>
        /// Adds a new quiz to the database asynchronously.
        /// </summary>
        public async Task AddAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing quiz in the database asynchronously.
        /// </summary>
        public async Task UpdateAsync(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a quiz by ID from the database asynchronously.
        /// Does nothing if the quiz does not exist.
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
        /// Checks if a quiz exists in the database by ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Quizzes.AnyAsync(q => q.QuizId == id);
        }
    }
}
