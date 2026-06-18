namespace QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;

public class SubmitAnswersRequestDto
{
    public required List<SubmitAnswerBody> Answers { get; set; }
}

public record SubmitAnswerBody
(
    Guid QuestionId,
    Guid SelectedOptionId,
    DateTimeOffset AnsweredAt
);
