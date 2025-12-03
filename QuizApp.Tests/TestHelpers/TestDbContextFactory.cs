using System;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;

namespace QuizApp.Tests.TestHelpers
{
    public static class TestDbContextFactory
    {
        public static QuizContext Create()
        {
            // Unique DB name per test to avoid collisions
            var options = new DbContextOptionsBuilder<QuizContext>()
                .UseInMemoryDatabase(databaseName: $"QuizApp_TestDb_{Guid.NewGuid()}")
                .Options;

            var context = new QuizContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
