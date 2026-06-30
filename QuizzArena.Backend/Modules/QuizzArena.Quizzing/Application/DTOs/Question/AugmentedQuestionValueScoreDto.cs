namespace QuizzArena.Quizzing.Application.DTOs.Question;

public record AugmentedQuestionDto(Domain.Entities.Question Question, decimal ValueScore);
