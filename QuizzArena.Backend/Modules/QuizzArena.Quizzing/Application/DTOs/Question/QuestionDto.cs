namespace QuizzArena.Quizzing.Application.DTOs.Question;

public class QuestionDto : BaseQuestionDto
{
    public bool WasModified { get; set; }
    public bool Deleted { get; set; }
}
