using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Domain.Entities
{
    public class Question
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public QuestionStatus Status { get; set; }
        public bool WasModified { get; set; }
        public QuestionType Type { get; set; }
        public bool Deleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public Guid? ProcessingJobId { get; set; }

        public ICollection<Option> Options { get; set; } = [];
    }
}