using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Data.Repositories.Interfaces;

namespace QuizApp.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository for Option database operations. Handles all data access for answer options.
    /// Options are the possible answers for a question. One option per question is marked as correct.
    /// </summary>
    public class OptionRepository : IOptionRepository
    {
        private readonly QuizContext _context;

        public OptionRepository(QuizContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a single option by ID, including its parent question and the quiz that question belongs to.
        /// Used when displaying or editing an option.
        /// </summary>
        public async Task<Option?> GetByIdAsync(int id)
        {
            return await _context.Options
                .Include(o => o.Question)
                .ThenInclude(q => q!.Quiz)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        /// <summary>
        /// Retrieves all answer options for a specific question.
        /// Used when displaying all options for a question. Uses AsNoTracking for read-only performance.
        /// </summary>
        public async Task<IEnumerable<Option>> GetByQuestionIdAsync(int questionId)
        {
            return await _context.Options
                .Where(o => o.QuestionId == questionId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new answer option to the database. Used when creating a new option for a question.
        /// </summary>
        public async Task AddAsync(Option option)
        {
            _context.Options.Add(option);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing option in the database. Used when editing an option's text or correct status.
        /// </summary>
        public async Task UpdateAsync(Option option)
        {
            _context.Options.Update(option);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an option from the database. If the option doesn't exist, does nothing (no error).
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var option = await _context.Options.FindAsync(id);
            if (option != null)
            {
                _context.Options.Remove(option);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks if an option with the given ID exists in the database.
        /// Used for validation before updating or deleting.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Options.AnyAsync(o => o.Id == id);
        }
    }
}
