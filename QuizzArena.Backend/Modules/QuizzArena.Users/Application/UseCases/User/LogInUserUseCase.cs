using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Users.Application.UseCases.User
{
    public class LogInUserUseCase(IUserRepository repository) : ILogInUserUseCase
    {
        public async Task<LogInResponseDto> Execute(LogInRequestDto dto)
        {
            return new LogInResponseDto();
        }
    }
}
