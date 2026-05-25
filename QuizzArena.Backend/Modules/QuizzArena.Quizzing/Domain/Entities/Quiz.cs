using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Domain.Entities
{
    public class Quiz
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public QuizStatus Status { get; set; }
        public bool Deleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public ICollection<QuizQuestion> QuizQuestions { get; set; } = []; 
    }
}