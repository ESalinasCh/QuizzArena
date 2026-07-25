using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Application.UseCases.QuizUseCases;

public class GetTeacherQuizzesUseCase(
    ICurrentUser currentUser,
    IMapper mapper,
    IQuizQueriesRepository quizRepo
    ) : IGetTeacherQuizzesUseCase
{
    public async Task<List<TeacherQuizResponseDto>> Execute(QuizOrigin? origin)
    {
        Guid teacherId = Guid.Parse(currentUser.UserId);
        List<Quiz> quizzes = await quizRepo.GetByTeacherIdAsync(teacherId, origin);
        return mapper.Map<List<TeacherQuizResponseDto>>(quizzes);
    }
}
