using QuizzArena.Users.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Users.Application.Ports.In
{
    public interface ILogInUserUseCase
    {
        Task<LogInResponseDto> Execute(LogInRequestDto dto);
    }
}
