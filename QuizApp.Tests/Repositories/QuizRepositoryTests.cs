using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Data.Repositories.Implementations;
using QuizApp.Models;
using QuizApp.Tests.TestHelpers;
using Xunit;

namespace QuizApp.Tests.Repositories
{
    public class QuizRepositoryTests
    {
        [Fact]
        public async Task AddQuiz_ShouldAddQuiz()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz
            {
                Title = "Test Quiz",
                Description = "Test description"
            };

            await repo.AddAsync(quiz);

            Assert.Equal(1, await context.Quizzes.CountAsync());
            var fromDb = await context.Quizzes.SingleAsync();
            Assert.Equal("Test Quiz", fromDb.Title);
            Assert.Equal("Test description", fromDb.Description);
        }

        [Fact]
        public async Task GetById_ShouldReturnQuiz_WhenExists()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz { Title = "Existing", Description = "Desc" };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(quiz.QuizId);

            Assert.NotNull(result);
            Assert.Equal(quiz.QuizId, result!.QuizId);
            Assert.Equal("Existing", result.Title);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenNotFound()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllQuizzes()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            context.Quizzes.AddRange(
                new Quiz { Title = "Q1", Description = "D1" },
                new Quiz { Title = "Q2", Description = "D2" }
            );
            await context.SaveChangesAsync();

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, q => q.Title == "Q1");
            Assert.Contains(result, q => q.Title == "Q2");
        }

        [Fact]
        public async Task UpdateQuiz_ShouldChangeValues()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz { Title = "Old", Description = "Old desc" };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            quiz.Title = "New";
            quiz.Description = "New desc";

            await repo.UpdateAsync(quiz);

            var fromDb = await context.Quizzes.FindAsync(quiz.QuizId);
            Assert.NotNull(fromDb);
            Assert.Equal("New", fromDb!.Title);
            Assert.Equal("New desc", fromDb.Description);
        }

        [Fact]
        public async Task DeleteQuiz_ShouldRemoveQuiz_WhenExists()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz { Title = "ToDelete", Description = "D" };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            await repo.DeleteAsync(quiz.QuizId);

            Assert.Equal(0, await context.Quizzes.CountAsync());
        }

        [Fact]
        public async Task DeleteQuiz_ShouldDoNothing_WhenQuizDoesNotExist()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            await repo.DeleteAsync(12345);

            Assert.Equal(0, await context.Quizzes.CountAsync());
        }

        [Fact]
        public async Task AddQuiz_ShouldThrow_WhenTitleMissing()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz
            {
                Title = null!,
                Description = "No title"
            };

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await repo.AddAsync(quiz);
            });
        }
    }
}
