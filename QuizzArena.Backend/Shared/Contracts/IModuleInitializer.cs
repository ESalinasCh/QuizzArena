using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts
{
    public interface IModuleInitializer
    {
        void Initialize(IServiceProvider serviceProvider);
    }
}
