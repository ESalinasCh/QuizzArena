using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Users.Application.UseCases.User
{
    internal class LogInUserUseCase : ILogInUserUseCase
    {
        private readonly IUserRepository _repository;

        public LogInUserUseCase(IUserRepository repository) => _repository = repository;
    }
}
