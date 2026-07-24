namespace QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;

public record SubmitAnswerBody
(
    Guid QuestionId,
    IReadOnlyList<Guid> SelectedOptionIds,
    DateTimeOffset AnsweredAt
);
