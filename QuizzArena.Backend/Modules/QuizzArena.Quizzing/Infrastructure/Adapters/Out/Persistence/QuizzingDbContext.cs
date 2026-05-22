using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence
{
    public class QuizzingDbContext : DbContext
    {
        public QuizzingDbContext(DbContextOptions<QuizzingDbContext> options) : base(options)
        {

        }
        public DbSet<Option> Options { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<Answer> Answers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresEnum<MatchMode>(schema: QuizzingConstants.Schema, name: "match_mode");
            modelBuilder.HasPostgresEnum<MatchStatus>(schema: QuizzingConstants.Schema, name: "match_status");
            modelBuilder.HasPostgresEnum<QuestionStatus>(schema: QuizzingConstants.Schema, name: "question_status");
            modelBuilder.HasPostgresEnum<QuestionType>(schema: QuizzingConstants.Schema, name: "question_type");
            modelBuilder.HasPostgresEnum<QuizAttemptStatus>(schema: QuizzingConstants.Schema, name: "quiz_attempt_status");
            modelBuilder.HasPostgresEnum<QuizStatus>(schema: QuizzingConstants.Schema, name: "quiz_status");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuizzingDbContext).Assembly);
        }

    }
}
