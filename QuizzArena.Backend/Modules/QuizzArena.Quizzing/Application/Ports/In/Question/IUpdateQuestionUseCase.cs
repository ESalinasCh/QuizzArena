using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.Ports.In.Question;

public interface IUpdateQuestionUseCase
{
    Task<UpdateQuestionResponseDto> Execute(UpdateQuestionRequestDto dto);
}
