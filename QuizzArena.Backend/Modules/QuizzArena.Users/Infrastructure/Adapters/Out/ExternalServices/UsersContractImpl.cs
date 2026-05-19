using QuizzArena.Users.Application.Ports.Out;
using Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.ExternalServices
{
    public class UsersContractImpl(IUserRepository _userRepository) : IUsersContract
    {
        public async Task<bool> IsProfessor(Guid userId)
        {
            return true; // Retorna true si es profesor
        }
    }
}
