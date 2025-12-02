using QuizApp.Models;

namespace QuizApp.Data.Repositories.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(int id);
        Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId);
        Task AddAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
