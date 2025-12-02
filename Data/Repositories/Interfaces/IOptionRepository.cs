using QuizApp.Models;

namespace QuizApp.Data.Repositories.Interfaces
{
    public interface IOptionRepository
    {
        Task<Option?> GetByIdAsync(int id);
        Task<IEnumerable<Option>> GetByQuestionIdAsync(int questionId);
        Task AddAsync(Option option);
        Task UpdateAsync(Option option);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
