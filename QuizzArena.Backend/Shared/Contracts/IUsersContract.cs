using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts
{
    public interface IUsersContract
    {
        Task<bool> IsProfessor(Guid userId);
    }
}
