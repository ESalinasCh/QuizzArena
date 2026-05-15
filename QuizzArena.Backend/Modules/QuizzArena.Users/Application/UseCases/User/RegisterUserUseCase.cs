using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Users.Application.UseCases.User
{
    internal class RegisterUserUseCase : ISignUpUserUseCase
    {
        private readonly IUserRepository _repository;

        public RegisterUserUseCase(IUserRepository repository) => _repository = repository;

        public async Task<SignUpResponseDto> Execute(SignUpRequestDto dto)
        {
            return new SignUpResponseDto();
        }
    }
}
