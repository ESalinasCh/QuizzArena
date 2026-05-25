using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Domain.Entities
{
    public class QuizAttempt
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }
        public DateTimeOffset JoinedAt { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public QuizAttemptStatus Status { get; set; }
        public int Score { get; set; }

        public Guid UserId { get; set; }
        public Guid MatchId { get; set; }

        public ICollection<Answer> Answers { get; set; } = [];
    }
}
