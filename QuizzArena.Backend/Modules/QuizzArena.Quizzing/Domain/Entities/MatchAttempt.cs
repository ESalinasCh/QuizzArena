using System.ComponentModel.DataAnnotations.Schema;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Domain.Entities;

public class MatchAttempt
{
    public Guid Id { get; set; }
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset? EndDateTime { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;
    public decimal Score { get; set; }

    public Guid UserId { get; set; }
    public Guid MatchId { get; set; }

    public ICollection<Answer> Answers { get; set; } = [];
    public ICollection<MatchAttemptQuestion> MatchAttemptQuestions { get; set; } = [];

    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    [NotMapped]
    public List<MatchAttempt> OtherAttempts { get; set; } = [];
}

