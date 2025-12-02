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
        // POSITIVE: Create quiz
        [Fact]
        public async Task AddQuiz_ShouldAddQuiz()
        {
            // arrange
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz
            {
                Title = "Test Quiz",
                Description = "Test description"
            };

            // act
            await repo.AddAsync(quiz);

            // assert
            Assert.Equal(1, await context.Quizzes.CountAsync());
            var fromDb = await context.Quizzes.SingleAsync();
            Assert.Equal("Test Quiz", fromDb.Title);
            Assert.Equal("Test description", fromDb.Description);
        }

        // POSITIVE: Get quiz by ID when it exists
        [Fact]
        public async Task GetById_ShouldReturnQuiz_WhenExists()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz { Title = "Existing", Description = "Desc" };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            // act
            var result = await repo.GetByIdAsync(quiz.QuizId);

            // assert
            Assert.NotNull(result);
            Assert.Equal(quiz.QuizId, result!.QuizId);
            Assert.Equal("Existing", result.Title);
        }

        // NEGATIVE: Get by ID when quiz does NOT exist
        [Fact]
        public async Task GetById_ShouldReturnNull_WhenNotFound()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            // act
            var result = await repo.GetByIdAsync(999);

            // assert
            Assert.Null(result);
        }

        // POSITIVE: Get all quizzes
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

            // act
            var result = await repo.GetAllAsync();

            // assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, q => q.Title == "Q1");
            Assert.Contains(result, q => q.Title == "Q2");
        }

        // POSITIVE: Update quiz
        [Fact]
        public async Task UpdateQuiz_ShouldChangeValues()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz { Title = "Old", Description = "Old desc" };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            // modify
            quiz.Title = "New";
            quiz.Description = "New desc";

            // act
            await repo.UpdateAsync(quiz);

            // assert
            var fromDb = await context.Quizzes.FindAsync(quiz.QuizId);
            Assert.NotNull(fromDb);
            Assert.Equal("New", fromDb!.Title);
            Assert.Equal("New desc", fromDb.Description);
        }

        // POSITIVE: Delete quiz that exists
        [Fact]
        public async Task DeleteQuiz_ShouldRemoveQuiz_WhenExists()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz { Title = "ToDelete", Description = "D" };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            // act
            await repo.DeleteAsync(quiz.QuizId);

            // assert
            Assert.Equal(0, await context.Quizzes.CountAsync());
        }

        // NEGATIVE: Delete quiz that does NOT exist
        [Fact]
        public async Task DeleteQuiz_ShouldDoNothing_WhenQuizDoesNotExist()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            // act
            await repo.DeleteAsync(12345);

            // assert
            Assert.Equal(0, await context.Quizzes.CountAsync());
        }

        // NEGATIVE: Adding quiz with missing required fields (e.g., Title)
        // This assumes Title is [Required] in your Quiz model.
        [Fact]
        public async Task AddQuiz_ShouldThrow_WhenTitleMissing()
        {
            using var context = TestDbContextFactory.Create();
            var repo = new QuizRepository(context);

            var quiz = new Quiz
            {
                Title = null!,                // assuming [Required] on Title
                Description = "No title"
            };

            // act + assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await repo.AddAsync(quiz);
            });
        }
    }
}
