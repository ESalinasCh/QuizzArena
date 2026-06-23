using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;

public class GetQuestionsUseCase(
    IQuestionRepository questionRepository,
    IMapper mapper
) : IGetQuestionsUseCase
{
    public async Task<List<ResponseQuestionDto>> Execute(QuestionFilters filters)
    {
        List<Question> questions = await questionRepository.GetByProcessingJobIdAsync(filters);
        return questions.Select(q => mapper.Map<ResponseQuestionDto>(q)).ToList();
    }
}
