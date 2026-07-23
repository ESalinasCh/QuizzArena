using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IGetQuizzesUseCase
{
    Task<List<CreateQuizResponseDto>> Execute(PagedRequest query);
}
