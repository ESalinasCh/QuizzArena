using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;

internal class QuizzingDbContext : DbContext
{
    public QuizzingDbContext(DbContextOptions<QuizzingDbContext> options) : base(options)
    {

    }
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchAttempt> MatchAttempts => Set<MatchAttempt>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<MatchAttemptQuestion> MatchAttemptQuestions => Set<MatchAttemptQuestion>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<MatchMode>(schema: QuizzingConstants.Schema, name: "match_mode");
        modelBuilder.HasPostgresEnum<MatchStatus>(schema: QuizzingConstants.Schema, name: "match_status");
        modelBuilder.HasPostgresEnum<QuestionStatus>(schema: QuizzingConstants.Schema, name: "question_status");
        modelBuilder.HasPostgresEnum<QuestionType>(schema: QuizzingConstants.Schema, name: "question_type");
        modelBuilder.HasPostgresEnum<QuizAttemptStatus>(schema: QuizzingConstants.Schema, name: "quiz_attempt_status");
        modelBuilder.HasPostgresEnum<QuizStatus>(schema: QuizzingConstants.Schema, name: "quiz_status");
        modelBuilder.HasPostgresEnum<QuizOrigin>(schema: QuizzingConstants.Schema, name: "quiz_origin");
        modelBuilder.HasPostgresEnum<QuestionOrigin>(schema: QuizzingConstants.Schema, name: "question_origin");


        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuizzingDbContext).Assembly);
    }

}
