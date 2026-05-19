using QuizzArena.Users.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence
{
    public class SqlUserRepository : IUserRepository
    {
        public async Task<User> GetById(Guid id)
        {
            //throw new NotImplementedException();
            return new Student();
        }

        public async Task Register(User quiz)
        {
            //throw new NotImplementedException();
            return;
        }

        public async Task Delete(Guid id)
        {
            //throw new NotImplementedException();
            return;
        }
    }
}
