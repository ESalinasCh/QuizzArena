using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Application.UseCases.QuizUseCases;

public class GetTeacherQuizzesUseCase(
    IValidator<QuizQueryParametersDto> queryValidator,
    ICurrentUser currentUser,
    IMapper mapper,
    IQuizQueriesRepository quizRepo
    ) : IGetTeacherQuizzesUseCase
{
    public async Task<List<TeacherQuizResponseDto>> Execute(QuizQueryParametersDto query)
    {
        await queryValidator.ValidateAndThrowAsync(query);

        Guid teacherId = Guid.Parse(currentUser.UserId);
        List<Quiz> quizzes = await quizRepo.GetByTeacherIdAsync(teacherId, query);
        return mapper.Map<List<TeacherQuizResponseDto>>(quizzes);
    }
}
