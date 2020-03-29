using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public interface IRepositoryWrapper
    {
        IAppUserRepository User { get; }
        void Save();
    }
}
