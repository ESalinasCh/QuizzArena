namespace QuizzArena.Quizzing.Domain.Entities
{
    public class Option
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Position { get; set; }
        public bool Deleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public Guid QuestionId { get; set; }
    }
}
