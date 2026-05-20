using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out;
using Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz
{
    public class CreateQuizUseCase(IQuizRepository repository, IUsersContract _userContract) : ICreateQuizUseCase
    {
        public async Task<CreateQuizResponseDto> Execute(CreateQuizRequestDto dto)
        {
            bool isProfessor = await _userContract.IsProfessor(Guid.NewGuid()); // Communicate with User Module through Shared Contract
            if (!isProfessor) throw new Exception("Only professors can create quizzes.");

            return new CreateQuizResponseDto();
        }
    }
}
