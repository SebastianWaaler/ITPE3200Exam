using System;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;

namespace QuizApp.Tests.TestHelpers
{
    public static class TestDbContextFactory
    {
        public static QuizContext Create()
        {
            // Use a unique DB name per test run so tests never collide
            var options = new DbContextOptionsBuilder<QuizContext>()
                .UseInMemoryDatabase(databaseName: $"QuizApp_TestDb_{Guid.NewGuid()}")
                .Options;

            var context = new QuizContext(options);

            // Ensure database is created
            context.Database.EnsureCreated();

            return context;
        }
    }
}
