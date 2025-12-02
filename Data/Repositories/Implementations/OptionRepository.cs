using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Data.Repositories.Interfaces;

namespace QuizApp.Data.Repositories.Implementations
{
    public class OptionRepository : IOptionRepository
    {
        private readonly QuizContext _context;

        public OptionRepository(QuizContext context)
        {
            _context = context;
        }

        public async Task<Option?> GetByIdAsync(int id)
        {
            return await _context.Options
                .Include(o => o.Question)
                .ThenInclude(q => q.Quiz)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Option>> GetByQuestionIdAsync(int questionId)
        {
            return await _context.Options
                .Where(o => o.QuestionId == questionId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Option option)
        {
            _context.Options.Add(option);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Option option)
        {
            _context.Options.Update(option);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var option = await _context.Options.FindAsync(id);
            if (option != null)
            {
                _context.Options.Remove(option);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Options.AnyAsync(o => o.Id == id);
        }
    }
}
