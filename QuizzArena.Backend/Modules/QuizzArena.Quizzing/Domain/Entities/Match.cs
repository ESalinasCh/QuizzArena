using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Domain.Entities
{
    public class Match
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public MatchStatus Status { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public MatchMode Mode { get; set; }
        public int TimeMinutes { get; set; }

        public bool Deleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public Guid CourseId { get; set; }
        public Guid QuizId { get; set; }
    }
}
