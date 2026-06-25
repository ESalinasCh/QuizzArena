namespace QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;

public record SubmitAnswerBody
(
    Guid QuestionId,
    Guid SelectedOptionId,
    DateTimeOffset AnsweredAt
);
